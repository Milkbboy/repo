using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang.Test
{
    public class CardUIHelper : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        private Coroutine flashCoroutine;
        Color color;

        public void SetSelfSlot(BoardSlot slot, Color? color = null)
        {
            if (color.HasValue)
                this.color = color.Value;

            StartFlashing();
        }

        public void SetTargetSlot(BoardSlot slot, Color? color = null)
        {
            if (color.HasValue)
                this.color = color.Value;

            StartFlashing();
        }

        public void Initialize(MeshRenderer meshRenderer)
        {
            this.meshRenderer = meshRenderer;
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
                SetColor(color); // 빨간색으로 변경
                yield return new WaitForSeconds(0.5f);
                ResetColor(); // 원래 색으로 복구
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void SetColor(Color color)
        {
            if (meshRenderer != null && meshRenderer.materials.Length > 0)
            {
                meshRenderer.materials[0].color = color;
            }
        }

        private void ResetColor()
        {
            if (meshRenderer != null && meshRenderer.materials.Length > 0)
            {
                meshRenderer.materials[0].color = Color.white;
            }
        }
    }
}