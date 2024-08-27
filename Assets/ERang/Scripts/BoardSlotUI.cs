using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BoardSlotUI : MonoBehaviour
    {
        [Header("Display")]
        public Texture2D cardTexture;
        public MeshRenderer cardMeshRenderer;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
        }
    }
}