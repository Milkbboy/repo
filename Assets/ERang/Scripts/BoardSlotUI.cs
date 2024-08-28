using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BoardSlotUI : MonoBehaviour
    {
        [Header("Display")]
        public Texture2D cardTexture;
        public MeshRenderer meshRenderer;

        private Coroutine flashCoroutine;
        private Color flashColor;

        // Start is called before the first frame update
        void Start()
        {
            flashColor = Color.blue;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartFlashing(Color? color = null)
        {
            if (color.HasValue)
                flashColor = color.Value;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            // 멈추기 위해 flashCoroutine 변수에 할당
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

        public void SetSlotType(CardType cardType)
        {
            // CardType.Master, CardType.Creature, CardType.None, CardType.Monster, CardType.EnemyMaster
            switch (cardType)
            {
                case CardType.None: cardTexture = Resources.Load<Texture2D>("Textures/Blank_Red"); break;
                case CardType.Creature: cardTexture = Resources.Load<Texture2D>("Textures/Blank_Green"); break;
                case CardType.Monster: cardTexture = Resources.Load<Texture2D>("Textures/Blank_Purple"); break;
            }

            if (cardTexture != null)
                meshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
        }

        private IEnumerator FlashRoutine()
        {
            while (true)
            {
                SetColor();
                yield return new WaitForSeconds(0.5f);

                ResetColor();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void SetColor()
        {
            if (meshRenderer != null && meshRenderer.materials.Length > 0)
            {
                meshRenderer.materials[0].color = flashColor;
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