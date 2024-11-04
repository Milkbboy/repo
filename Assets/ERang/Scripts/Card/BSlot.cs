using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BSlot : MonoBehaviour
    {
        private BaseCard card;
        private CardUI cardUI;

        // Start is called before the first frame update
        void Start()
        {
            cardUI = GetComponent<CardUI>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void EquipCard(BaseCard card)
        {
            this.card = card;
            cardUI.SetCard(card);
        }
    }
}
