using UnityEngine;
using TMPro;
using ERang.Data;

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
        public GameObject statObj;

        void Start()
        {
            // statObj.SetActive(false);
        }

        public void SetCard(Card card)
        {
            // Debug.Log("CardUI SetCard: " + cardId);
            CardData cardData = CardData.GetCardData(card.id);
            Texture2D cardTexture = cardData.GetCardTexture();

            if (!cardTexture)
            {
                Debug.LogError($"${cardData.card_id} Card texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
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
            Debug.Log("CardUI SetMasterCard: " + master.masterId);
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

            SetMasterStat(master);
        }

        public void SetMasterStat(Master master)
        {
            if (descText != null)
            {
                descText.text = $"hp: {master.hp}\natk: {master.atk}\nmana: {master.Mana}/{master.MaxMana}";
            }

            hpText.text = master.hp.ToString();
            manaText.text = master.Mana.ToString();
            atkText.text = master.atk.ToString();
        }
    }
}