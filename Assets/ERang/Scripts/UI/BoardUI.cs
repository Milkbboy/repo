using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ERang
{
    public class BoardUI : MonoBehaviour
    {
        public TextMeshProUGUI turnCount;
        public TextMeshProUGUI gold;

        /// <summary>
        /// 턴 카운트 설정
        /// </summary>
        /// <param name="count"></param>
        public void SetTurnCount(int count)
        {
            turnCount.text = count.ToString();
        }

        /// <summary>
        /// 골드 설정
        /// </summary>
        /// <param name="gold"></param>
        public void SetGold(int gold)
        {
            this.gold.text = gold.ToString();
        }
    }
}