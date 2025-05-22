using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    /// <summary>
    /// 스킬 카드 소환으로 생성되는 카드
    /// </summary>
    public class SummonCard : MonoBehaviour
    {
        private CardUI cardUI;
        private DiscardAnimation discardAnimation;

        void Awake()
        {
            cardUI = GetComponent<CardUI>();
            discardAnimation = GetComponent<DiscardAnimation>();
        }

        public void SetCard(GameCard card)
        {
            cardUI.SetCard(card);
        }
    }
}