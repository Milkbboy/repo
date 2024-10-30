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

        private CardUI cardUI;
        private Ani_Attack aniAttack;
        private Ani_Damaged aniDamaged;

        private Coroutine flashCoroutine;
        private Color flashColor = Color.blue;

        void Awake()
        {
            cardUI = GetComponent<CardUI>();
            cardUI.cardObject.SetActive(false);

            aniAttack = GetComponent<Ani_Attack>();
            aniDamaged = GetComponent<Ani_Damaged>();
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

            aniAttack.isAttackingFromLeft = cardType == CardType.Master || cardType == CardType.Creature;
        }

        public void SetCard(Card card)
        {
            cardUI.cardObject.SetActive(true);
            cardUI.SetCard(card);
        }

        public void SetDesc(int slot, int index)
        {
            cardUI.SetDesc($"Slot: {slot}, Index: {index}");
        }

        public void SetMasterCard(Master master)
        {
            cardUI.cardObject.SetActive(true);
            cardUI.SetMasterCard(master);
        }

        public void SetEnemyMasterCard(Enemy enemy)
        {
            cardUI.cardObject.SetActive(true);
            cardUI.SetEnemyMasterCard(enemy);
        }

        public void SetMana(int mana)
        {
            cardUI.SetMana(mana);
        }

        public void SetHp(int hp)
        {
            cardUI.SetHp(hp);
        }

        public void SetAtk(int atk)
        {
            cardUI.SetAtk(atk);
        }

        public void SetDef(int def)
        {
            cardUI.SetDef(def);
        }

        public void SetHpDef(int hp, int def)
        {
            cardUI.SetHp(hp);
            cardUI.SetDef(def);
        }

        public void SetFloatingGold(int beforeGold, int afterGold)
        {
            cardUI.SetFloatingGold(beforeGold, afterGold);
        }

        public void SetResetStat()
        {
            cardUI.ResetStat();
            cardUI.cardObject.SetActive(false);
        }

        public void AniAttack()
        {
            aniAttack.PlaySequence();
        }

        public void AniDamaged()
        {
            aniDamaged.PlaySequence();
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