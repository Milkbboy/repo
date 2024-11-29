using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ERang
{
    public class SatietyUI : MonoBehaviour
    {
        public Scrollbar satietyBar;
        public TextMeshProUGUI satietyText;
        public TextMeshProUGUI maxSatietyText;
        public float animationDuration = 0.5f;

        private Coroutine currentAnimation;

        public void UpdateSatiety(int satiety, int maxSatiety)
        {
            float targetSize = (float)satiety / maxSatiety;
            satietyText.text = satiety.ToString();
            maxSatietyText.text = maxSatiety.ToString();

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(AnimateSatietyBar(targetSize));
        }

        private IEnumerator AnimateSatietyBar(float targetSize)
        {
            float startSize = satietyBar.size;
            float startY = satietyText.rectTransform.anchoredPosition.y;
            float targetY = Mathf.Lerp(satietyBar.GetComponent<RectTransform>().rect.yMin, satietyBar.GetComponent<RectTransform>().rect.yMax, targetSize);
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                satietyBar.size = Mathf.Lerp(startSize, targetSize, elapsedTime / animationDuration);

                float newY = Mathf.Lerp(startY, targetY, elapsedTime / animationDuration);
                satietyText.rectTransform.anchoredPosition = new Vector2(satietyText.rectTransform.anchoredPosition.x, newY);

                yield return null;
            }

            satietyBar.size = targetSize;
            satietyText.rectTransform.anchoredPosition = new Vector2(satietyText.rectTransform.anchoredPosition.x, targetY);

            // 슬라이딩 바의 Image 컴포넌트를 제어하여 보이지 않도록 설정
            if (satietyBar.handleRect != null)
            {
                Image handleImage = satietyBar.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.enabled = targetSize > 0;
                }
            }
        }
    }
}