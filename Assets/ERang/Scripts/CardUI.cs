using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ERang.Data;

namespace ERang
{
    public class CardUI : MonoBehaviour
    {
        public MeshRenderer cardMeshRenderer;

        public void SetCard(int cardId)
        {
            Debug.Log("CardUI SetCard: " + cardId);
            CardData cardData = CardData.GetCardData(cardId);
            Texture2D cardTexture = cardData.GetCardTexture();

            if (!cardTexture)
            {
                Debug.LogError($"${cardData.card_id} Card texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", cardData.GetCardTexture());
            }
        }
    }
}