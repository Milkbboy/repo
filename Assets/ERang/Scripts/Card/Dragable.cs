using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace ERang
{
    public class Dragable : MonoBehaviour
    {
        /// <summary>
        /// 드래그 상태를 외부에서 확인할 수 있는 프로퍼티
        /// </summary>
        public bool IsDragging => isDragging;
        public Vector3 OriginalPosition => originalPosition;

        public float hoverHeight = 1f;
        public float animationDuration = 0.1f;
        public float scaleFactor = 1.5f;
        public GameObject cardGlow;

        private bool isDragging = false;
        private bool isCentered = false;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        // 마우스 다운 시의 오프셋
        private Vector3 mouseOffset;
        // y 방향으로 이동할 거리 임계값
        private float dragThreshold = 1f;
        private float initialYPosition;

        private int originalSortingOrder;
        private int originalTextSortingOrder;

        private Renderer[] renderers;
        private TextMeshPro[] textMeshPros;

        private Material cardGlowMaterial;
        private Coroutine glowCoroutine;

        private void SetTransparentMaterial(Material mat)
        {
            if (mat == null) return;

            // Surface(0=Opaque, 1=Transparent)
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 2989; // Transparent
        }

        void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            textMeshPros = GetComponentsInChildren<TextMeshPro>(includeInactive: true);

            // TextMeshPro의 MeshRenderer를 제외한 Renderer 배열 생성
            renderers = System.Array.FindAll(renderers, r => !(r is MeshRenderer && r.GetComponent<TextMeshPro>() != null));

            // Debug.Log($"textMeshPros.Length: {textMeshPros.Length}");

            cardGlow?.SetActive(false);

            if (cardGlow != null)
            {
                cardGlowMaterial = cardGlow.GetComponent<Renderer>().material;

                if (cardGlowMaterial == null)
                {
                    Debug.LogError("CardGlow 오브젝트에 Renderer 컴포넌트가 없거나 Material이 없습니다.");
                }

                if (cardGlowMaterial != null)
                {
                    SetTransparentMaterial(cardGlowMaterial);
                }
            }
        }

        void Start()
        {
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }

        void OnMouseDown()
        {
            HCard hCard = GetComponent<HCard>();

            // 핸드 온 카드 드래깅 안되게 처리
            if (hCard.IsHandOnCard())
                return;

            isDragging = true;
            // 드래그 시작 시 y 위치 저장
            initialYPosition = transform.position.y;

            // 마우스 다운 시의 오프셋 계산
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            mouseOffset = transform.position - objPosition;

            // Debug.Log($"Draggable. 현재 위치: {transform.position}, originalPosition: {originalPosition}");
        }

        void OnMouseDrag()
        {
            if (isDragging == false)
                return;

            if (isCentered)
                return;

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z);
            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            transform.position = objPosition + mouseOffset;

            // y 방향으로 일정 거리 이상 이동했는지 확인
            if (Mathf.Abs(transform.position.y - initialYPosition) >= dragThreshold)
            {
                HCard hCard = GetComponent<HCard>();

                // 매직 카드인 경우 중앙으로 이동
                if (hCard.Card is MagicCard magicCard && magicCard.IsSelectAttackType)
                {
                    MoveCardToCenter();

                    HandDeck.Instance?.SetTargettingArraow(true);
                }
            }
        }

        void OnMouseUp()
        {
            isDragging = false;
            isCentered = false;

            transform?.DOScale(originalScale, animationDuration);

            ResetSortingOrder();
        }

        void OnMouseEnter()
        {
            if (HandDeck.Instance?.DraggingCard != null)
                return;

            if (isDragging)
                return;

            cardGlow?.SetActive(true);

            if (glowCoroutine == null && cardGlowMaterial != null)
            {
                glowCoroutine = StartCoroutine(GlowEffect());
            }

            transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
            transform.DOScale(originalScale * scaleFactor, animationDuration);

            // 모든 렌더러의 sortingOrder를 높게 설정
            foreach (var renderer in renderers)
            {
                originalSortingOrder = renderer.sortingOrder;
                renderer.sortingOrder = 1000; // 높은 값으로 설정하여 맨 앞으로 이동
            }

            // 모든 TextMeshPro의 sortingOrder를 높게 설정
            foreach (var textMeshPro in textMeshPros)
            {
                originalTextSortingOrder = textMeshPro.sortingOrder;
                textMeshPro.sortingOrder = 1001; // 높은 값으로 설정하여 맨 앞으로 이동
            }
        }

        void OnMouseExit()
        {
            if (HandDeck.Instance?.DraggingCard != null)
                return;

            if (isDragging)
                return;

            cardGlow?.SetActive(false);

            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;

                if (cardGlowMaterial != null)
                {
                    Color color = cardGlowMaterial.color;
                    color.a = 1.0f;
                    cardGlowMaterial.color = color;
                }
            }

            transform.DOMoveY(originalPosition.y, animationDuration);
            transform.DOScale(originalScale, animationDuration);

            ResetSortingOrder();
        }

        public void MoveToOriginalPosition()
        {
            transform.DOMove(originalPosition, animationDuration);
        }

        private IEnumerator GlowEffect()
        {
            while (true)
            {
                Color color = cardGlowMaterial.color;
                color.a = Mathf.PingPong(Time.time, .5f);
                cardGlowMaterial.color = color;
                yield return null; // 다음 프레임까지 대기
            }
        }

        private void ResetSortingOrder()
        {
            // 모든 렌더러의 sortingOrder를 원래 값으로 복원
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = originalSortingOrder;
            }

            // 모든 TextMeshPro의 sortingOrder를 원래 값으로 복원
            foreach (var textMeshPro in textMeshPros)
            {
                textMeshPro.sortingOrder = originalTextSortingOrder;
            }
        }

        private void MoveCardToCenter()
        {
            isCentered = true;

            transform.DOMove(new Vector3(0, initialYPosition, originalPosition.z), animationDuration);
        }
    }
}