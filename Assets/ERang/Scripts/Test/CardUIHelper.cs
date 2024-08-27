using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang.Test
{
    public class CardUIHelper : MonoBehaviour
    {
        private MeshRenderer cardMeshRenderer;
        private Coroutine flashCoroutine;
        Color color;

        public void Initialize(MeshRenderer meshRenderer)
        {
            cardMeshRenderer = meshRenderer;
            color = Color.red;
        }

        public void StartFlashing(Color? color = null)
        {
            if (color.HasValue)
                this.color = color.Value;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        public void StopFlashing()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
                ResetColor();
            }
        }

        private IEnumerator FlashRoutine()
        {
            while (true)
            {
                SetColor(Color.blue); // 빨간색으로 변경
                yield return new WaitForSeconds(0.5f);
                ResetColor(); // 원래 색으로 복구
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void SetColor(Color color)
        {
            if (cardMeshRenderer != null && cardMeshRenderer.materials.Length > 0)
            {
                cardMeshRenderer.materials[0].color = color;
            }
        }

        private void ResetColor()
        {
            if (cardMeshRenderer != null && cardMeshRenderer.materials.Length > 0)
            {
                cardMeshRenderer.materials[0].color = Color.white;
            }
        }
    }
}