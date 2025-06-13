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

            mainCamera = Camera.main;
            boardSlotLayer = 1 << LayerMask.NameToLayer("BoardSlot");

            renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var part in arrows)
            {
                part.SetActive(false);
            }

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

            foreach (var part in arrows)
            {
                part.SetActive(arrowEnabled);
            }

            if (!arrowEnabled)
            {
                UnselectEnemy();
            }

            if (isArrowEnabled == true)
            {
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var renderer = renderers[i];
                    originalSortingOrder = renderer.sortingOrder;
                    renderer.sortingOrder = 2000 + i;
                }
            }
        }

        private void LateUpdate()
        {
            if (!isArrowEnabled)
            {
                return;
            }

            HandleTargetSelection();
            DrawArrow();
        }

        /// <summary>
        /// 타겟 선택 처리 - 기존 로직 복원 (타겟 락 제거)
        /// </summary>
        private void HandleTargetSelection()
        {
            var mousePos = Input.mousePosition;
            float cameraToPlaneDistance = Mathf.Abs(mainCamera.transform.position.z);
            mousePos.z = cameraToPlaneDistance;

            var ray = mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, boardSlotLayer))
            {
                if (hitInfo.collider.gameObject != selectedSlot || selectedSlot == null)
                {
                    BSlot boardSlot = hitInfo.collider.gameObject.GetComponent<BSlot>();

                    if (HandDeck.Instance.IsTargetSlot(boardSlot.SlotNum))
                    {
                        Debug.Log($"🔍 HandleTargetSelection: 타겟 슬롯 {boardSlot.ToSlotLogInfo()} 선택!");
                        SelectEnemy(hitInfo.collider.gameObject);
                    }
                    else
                    {
                        Debug.Log($"🔍 HandleTargetSelection: 슬롯 {boardSlot.ToSlotLogInfo()}는 타겟 불가능, 선택 해제");
                        UnselectEnemy();
                    }
                }
            }
            else
            {
                UnselectEnemy();
            }
        }

        /// <summary>
        /// 화살표 그리기
        /// </summary>
        private void DrawArrow()
        {
            var mousePos = Input.mousePosition;
            float cameraToPlaneDistance = Mathf.Abs(mainCamera.transform.position.z);
            mousePos.z = cameraToPlaneDistance;

            var worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);
            var mouseX = worldMousePos.x;
            var mouseY = worldMousePos.y;

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
            }
        }

        private void SetVertexColor(GameObject vertext, Color color)
        {
            var renderer = vertext.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }

        private void SelectEnemy(GameObject go)
        {
            selectedSlot = go;
            selectedSlotNum = go.GetComponent<BSlot>().SlotNum;

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

        private void UnselectEnemy()
        {
            if (selectedSlot == null)
                return;

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

        /// <summary>
        /// 🔧 마우스 업 시점에 현재 마우스 위치에 있는 슬롯 번호 반환
        /// </summary>
        public int GetCurrentMouseOverSlotNum()
        {
            if (!isArrowEnabled)
                return -1;

            var mousePos = Input.mousePosition;
            float cameraToPlaneDistance = Mathf.Abs(mainCamera.transform.position.z);
            mousePos.z = cameraToPlaneDistance;

            var ray = mainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, boardSlotLayer))
            {
                BSlot boardSlot = hitInfo.collider.gameObject.GetComponent<BSlot>();
                return boardSlot.SlotNum;
            }

            return -1;
        }
    }
}