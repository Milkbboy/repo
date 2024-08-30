using UnityEngine;
using TMPro;
using ERang.Data;
using System.Collections;
using ERang.Test;

namespace ERang
{
    public class CardUI : MonoBehaviour
    {
        public MeshRenderer cardMeshRenderer;
        public TextMeshProUGUI cardTypeText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI manaText;
        public TextMeshProUGUI atkText;
        public TextMeshProUGUI defText;
        public GameObject cardObject;

        private Texture2D originTexture;

        void Awake()
        {
        }

        void Start()
        {
        }

        public void SetCard(Card card)
        {
            // Debug.Log("CardUI SetCard: " + cardId);
            CardData cardData = (card.type == CardType.Monster) ? MonsterCardData.GetCardData(card.id) : CardData.GetCardData(card.id);
            Texture2D cardTexture = cardData.GetCardTexture();

            if (!cardTexture)
            {
                Debug.LogError($"${cardData.card_id} Card texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                // 원래 텍스쳐 저장
                if (originTexture == null)
                {
                    originTexture = (Texture2D)cardMeshRenderer.materials[0].GetTexture("_BaseMap");
                }

                cardMeshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = card.type.ToString();
            }

            // 카드 정보 표시
            if (descText != null)
            {
                descText.text = $"gold: {card.costGold.ToString()}\nmana: {card.costMana.ToString()}";

                if (card.hp > 0)
                {
                    descText.text += $"\nhp: {card.hp}";
                }

                if (card.atk > 0)
                {
                    descText.text += $"\natk: {card.atk}";
                }
            }

            hpText.text = card.hp.ToString();
            manaText.text = card.costMana.ToString();
            atkText.text = card.atk.ToString();
            defText.text = card.def.ToString();
        }

        public void SetMasterCard(Master master)
        {
            // Debug.Log("CardUI SetMasterCard: " + master.masterId);
            MasterData masterData = MasterData.master_dict[master.masterId];

            // Debug.Log("CardUI SetCard: " + cardId);
            Texture2D masterTexture = masterData.GetMasterTexture();

            if (!masterTexture)
            {
                Debug.LogError($"${master.masterId} Master texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", masterTexture);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = "Master";
            }

            SetMasterStat(master.Hp, master.MaxHp, master.Atk, master.Def, master.Mana, master.MaxMana);
        }

        public void SetEnemyMasterCard(Enemy enemy)
        {
            // Debug.Log("CardUI SetEnemyMasterCard: " + enemy.enemyId);
            MasterData enemyData = MasterData.master_dict[enemy.enemyId];

            // Debug.Log("CardUI SetCard: " + cardId);
            Texture2D enemyTexture = enemyData.GetMasterTexture();

            if (!enemyTexture)
            {
                Debug.LogError($"${enemy.enemyId} Enemy Master texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", enemyTexture);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = "Enemy Master";
            }

            SetMasterStat(enemy.Hp, enemy.MaxHp, enemy.Atk, enemy.Def);
        }

        public void SetMasterStat(int hp, int maxHp, int atk, int def, int mana = 0, int maxMana = 0)
        {
            if (descText != null)
            {
                descText.text = $"hp: {hp}/{maxHp}\nmana: {mana}/{maxMana}\natk: {atk}\ndef: {def}";
            }

            hpText.text = $"{hp}/{maxHp}";
            manaText.text = $"{mana}/{maxMana}";
            atkText.text = atk.ToString();
            defText.text = def.ToString();
        }

        public void SetHp(int hp)
        {
            hpText.text = hp.ToString();
        }

        public void SetAtk(int atk)
        {
            atkText.text = atk.ToString();
        }

        public void SetDef(int def)
        {
            defText.text = def.ToString();
        }

        public void ResetStat()
        {
            hpText.text = string.Empty;
            manaText.text = string.Empty;
            atkText.text = string.Empty;
            defText.text = string.Empty;

            // 원래 텍스쳐로 복구
            if (originTexture != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", originTexture);
            }
        }
    }
}