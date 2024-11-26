using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// The targeting arrow that allows the player to select an enemy as the target of
    /// the current card effect.
    /// </summary>
    public class TargetingArrow : MonoBehaviour
    {
        public int SelectedSlotNum => selectedSlotNum;
        [SerializeField]
        private GameObject bodyPrefab;
        [SerializeField]
        private GameObject headPrefab;

        [SerializeField]
        private GameObject topLeftPrefab;
        [SerializeField]
        private GameObject topRightPrefab;
        [SerializeField]
        private GameObject bottomLeftPrefab;
        [SerializeField]
        private GameObject bottomRightPrefab;

        private int selectedSlotNum = -1;
        private const int NumPartsTargetingArrow = 17;
        private readonly List<GameObject> arrows = new List<GameObject>(NumPartsTargetingArrow);

        private GameObject selectedSlot;
        private GameObject topLeftVertex;
        private GameObject topRightVertex;
        private GameObject bottomLeftVertex;
        private GameObject bottomRightVertex;

        private Camera mainCamera;

        private LayerMask boardSlotLayer;

        private bool isArrowEnabled;

        private int originalSortingOrder;
        private Renderer[] renderers;

        private void Start()
        {
            for (var i = 0; i < NumPartsTargetingArrow - 1; i++)
            {
                var body = Instantiate(bodyPrefab, gameObject.transform);
                arrows.Add(body);
            }

            var head = Instantiate(headPrefab, gameObject.transform);
            arrows.Add(head);

            topLeftVertex = Instantiate(topLeftPrefab, gameObject.transform);
            topRightVertex = Instantiate(topRightPrefab, gameObject.transform);
            bottomLeftVertex = Instantiate(bottomLeftPrefab, gameObject.transform);
            bottomRightVertex = Instantiate(bottomRightPrefab, gameObject.transform);

            DisableSelectionBox();

            mainCamera  = Camera.main;

            boardSlotLayer = 1 << LayerMask.NameToLayer("BoardSlot");

            // 모든 렌더러를 가져옵니다.
            renderers = GetComponentsInChildren<Renderer>(true);

            // Debug.Log($"renderers.Length: {renderers.Length}");

            foreach (var part in arrows)
            {
                part.SetActive(false);
            }

            // 각 모서리를 나타내는 Vertex들의 색을 약간 노란색으로 변경
            SetVertexColor(topLeftVertex, Color.yellow);
            SetVertexColor(topRightVertex, Color.yellow);
            SetVertexColor(bottomLeftVertex, Color.yellow);
            SetVertexColor(bottomRightVertex, Color.yellow);
        }

        public void EnableArrow(bool arrowEnabled)
        {
            if (isArrowEnabled == arrowEnabled)
                return;

            isArrowEnabled = arrowEnabled;

            Debug.Log($"EnableArrow called with: {isArrowEnabled}");

            foreach (var part in arrows)
            {
                part.SetActive(arrowEnabled);
            }

            if (!arrowEnabled)
            {
                UnselectEnemy(3);
            }

            // 모든 렌더러의 sortingOrder를 높게 설정
            if (isArrowEnabled == true)
            {
                foreach (var renderer in renderers)
                {
                    originalSortingOrder = renderer.sortingOrder;
                    renderer.sortingOrder = 2000; // 높은 값으로 설정하여 맨 앞으로 이동
                    // Debug.Log($"Renderer: {renderer.gameObject.name}, New SortingOrder: {renderer.sortingOrder}");
                }
            }
        }

        private void LateUpdate()
        {
            if (!isArrowEnabled)
            {
                return;
            }

            // 마우스 위치를 가져오고, z 값을 카메라와의 거리로 설정
            var mousePos = Input.mousePosition;

            // 카메라와의 적절한 거리 계산 (예: 카메라가 z = -10 위치에 있고, 게임 월드의 평면이 z = 0 위치에 있는 경우)
            float cameraToPlaneDistance = Mathf.Abs(mainCamera.transform.position.z);

            mousePos.z = cameraToPlaneDistance;

            var worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);
            var mouseX = worldMousePos.x;
            var mouseY = worldMousePos.y;

            // Debug.Log($"mousePos: {mousePos}, mouseX: {mouseX}, mouseY: {mouseY}");

            var ray = mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, boardSlotLayer))
            {
                // Debug.Log($"HitInfo: {hitInfo.collider.gameObject.name}");

                if (hitInfo.collider.gameObject != selectedSlot || selectedSlot == null)
                {
                    BSlot boardSlot = hitInfo.collider.gameObject.GetComponent<BSlot>();
                    // Debug.Log($"HitInfo: {hitInfo.collider.gameObject.name}, {(boardSlot.Card != null ? $"Card: {boardSlot.Card.LogText}" : "No Card")}");

                    if (HandDeck.Instance.IsTargetSlot(boardSlot.SlotNum))
                    {
                        SelectEnemy(hitInfo.collider.gameObject);
                    }
                    else
                    {
                        UnselectEnemy(1);
                    }
                }
            }
            else
            {
                UnselectEnemy(2);
            }

            const float centerX = 0.0f;
            const float centerY = -4.0f;

            var controlAx = centerX - (mouseX - centerX) * 0.3f;
            var controlAy = centerY + (mouseY - centerY) * 0.8f;
            var controlBx = centerX + (mouseX - centerX) * 0.1f;
            var controlBy = centerY + (mouseY - centerY) * 1.4f;

            for (var i = 0; i < arrows.Count; i++)
            {
                var part = arrows[i];

                var t = (i + 1) * 1.0f / arrows.Count;
                var tt = t * t;
                var ttt = tt * t;
                var u = 1.0f - t;
                var uu = u * u;
                var uuu = uu * u;

                var arrowX = uuu * centerX +
                             3 * uu * t * controlAx +
                             3 * u * tt * controlBx +
                             ttt * mouseX;
                var arrowY = uuu * centerY +
                             3 * uu * t * controlAy +
                             3 * u * tt * controlBy +
                             ttt * mouseY;

                arrows[i].transform.position = new Vector3(arrowX, arrowY, 0.0f);

                float lenX;
                float lenY;
                if (i > 0)
                {
                    lenX = arrows[i].transform.position.x - arrows[i - 1].transform.position.x;
                    lenY = arrows[i].transform.position.y - arrows[i - 1].transform.position.y;
                }
                else
                {
                    lenX = arrows[i + 1].transform.position.x - arrows[i].transform.position.x;
                    lenY = arrows[i + 1].transform.position.y - arrows[i].transform.position.y;
                }

                part.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(lenX, lenY) * Mathf.Rad2Deg);

                // part.transform.localScale = new Vector3(
                //     1.0f - 0.03f * (arrows.Count - 1 - i),
                //     1.0f - 0.03f * (arrows.Count - 1 - i),
                //     0);
            }
        }

        private void SetVertexColor(GameObject vertext, Color color)
        {
            var renderer = vertext.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
            else
            {
                Debug.Log($"Renderer is null for vertex: {vertext.name}");
            }
        }

        private void SelectEnemy(GameObject go)
        {
            selectedSlot = go;

            selectedSlotNum = go.GetComponent<BSlot>().SlotNum;
            Debug.Log($"{"SelectEnemy"}: {selectedSlot.name}, {selectedSlotNum}");

            var boxCollider = go.GetComponent<BoxCollider>();
            var size = boxCollider.size;
            var center = boxCollider.center;

            var topLeftLocal = center + new Vector3(-size.x * 0.5f, size.y * 0.5f, 0);
            var topLeftWorld = go.transform.TransformPoint(topLeftLocal);
            var topRightLocal = center + new Vector3(size.x * 0.5f, size.y * 0.5f, 0);
            var topRightWorld = go.transform.TransformPoint(topRightLocal);
            var bottomLeftLocal = center + new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0);
            var bottomLeftWorld = go.transform.TransformPoint(bottomLeftLocal);
            var bottomRightLocal = center + new Vector3(size.x * 0.5f, -size.y * 0.5f, 0);
            var bottomRightWorld = go.transform.TransformPoint(bottomRightLocal);

            EnableSelectionBox();

            topLeftVertex.transform.position = topLeftWorld;
            topRightVertex.transform.position = topRightWorld;
            bottomLeftVertex.transform.position = bottomLeftWorld;
            bottomRightVertex.transform.position = bottomRightWorld;
        }

        private void UnselectEnemy(int temp)
        {
            // Debug.Log($"<color=red>{selectedSlot?.name} unselected - {temp}</color>");
            selectedSlot = null;
            selectedSlotNum = -1;

            DisableSelectionBox();
        }

        private void EnableSelectionBox()
        {
            topLeftVertex.SetActive(true);
            topRightVertex.SetActive(true);
            bottomLeftVertex.SetActive(true);
            bottomRightVertex.SetActive(true);
        }

        private void DisableSelectionBox()
        {
            topLeftVertex.SetActive(false);
            topRightVertex.SetActive(false);
            bottomLeftVertex.SetActive(false);
            bottomRightVertex.SetActive(false);
        }
    }
}
