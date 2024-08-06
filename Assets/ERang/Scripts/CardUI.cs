using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ERang.Data;

namespace ERang
{
    public class CardUI : MonoBehaviour
    {
        public MeshRenderer cardMeshRenderer;
        public TextMeshProUGUI cardTypeText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI atkText;

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

            if (hpText != null)
            {
                hpText.text = $"{card.hp}";
            }

            if (atkText != null)
            {
                atkText.text = $"{card.atk}";
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = card.type.ToString();
            }
        }

        public void SetCard(Master master)
        {
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

            if (hpText != null)
            {
                hpText.text = $"{master.hp}";
            }

            if (atkText != null)
            {
                atkText.text = $"{master.atk}";
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = "Master";
            }
        }
    }
}