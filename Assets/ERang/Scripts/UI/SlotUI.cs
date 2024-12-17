using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class SlotUI : MonoBehaviour
    {
        [Header("Display")]
        public Texture2D cardTexture;
        public MeshRenderer meshRenderer;

        public void SetSlot(CardType cardType)
        {
            if (cardType == CardType.None)
                cardTexture = Resources.Load<Texture2D>("Textures/T_CardFrame_Back");

            if (cardTexture != null)
                meshRenderer.materials[0].SetTexture("_MainTex", cardTexture);
        }
    }
}