using System.Collections;
using System.Collections.Generic;
using ERang.Data;
using UnityEngine;

namespace ERang
{
    public class BattleLogic : MonoBehaviour
    {
        public HandDeck handDeck;

        public int turn = 0;
        public int masterId = 1001;

        // Start is called before the first frame update
        void Start()
        {
            // handDeck = GameObject.Find("HandDeck").GetComponent<HandDeck>();

            // 처음 시작이면 플레이어 startDeck 설정
            if (turn == 0)
            {
                MasterData masterData = MasterData.GetMasterData(masterId);

                handDeck.SpawnNewCards(masterData.startCardIds);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}