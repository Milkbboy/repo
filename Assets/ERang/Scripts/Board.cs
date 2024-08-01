using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ERang
{
    public class Board : MonoBehaviour
    {
        // Start is called before the first frame update
        public static Board Instance { get; private set; }

        public readonly CardType[] BoardSlotCardTypes = { CardType.Master, CardType.Creature, CardType.Creature, CardType.Creature, CardType.None, CardType.None, CardType.EnemyCreature, CardType.EnemyCreature, CardType.EnemyCreature, CardType.Master };

        public GameObject[] boardSlots;
        private float boardWidth = 0f;
        private float boardSpacing = 0.2f;

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            CreateBoardSlots();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void CreateBoardSlots()
        {
            // 보드 slot 구성
            // [ 0: Master, 1: Creature, 2: Creature, 3: Creature, 4: None, 
            //   5: None, 6: Creature, 7: Creature, 8: Creature, 9: Master ]
            boardWidth = boardSlots[0].GetComponent<BoxCollider>().size.x * boardSlots[0].transform.localScale.x;

            float totalWidth = (boardSlots.Length - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            for (int i = 0; i < boardSlots.Length; i++)
            {
                boardSlots[i].GetComponent<BoardSlot>().slot = i;
                boardSlots[i].GetComponent<BoardSlot>().cardType = BoardSlotCardTypes[i];
                float xPosition = startX + i * (boardWidth + boardSpacing);

                boardSlots[i].transform.position = new Vector3(xPosition, boardSlots[i].transform.position.y, boardSlots[i].transform.position.z);
            }
        }

        public BoardSlot NeareastBoardSlot(Vector3 position)
        {
            BoardSlot nearestSlot = null;
            float minDistance = float.MaxValue;

            foreach (GameObject slot in boardSlots)
            {
                BoardSlot BoardSlot = slot.GetComponent<BoardSlot>();

                // Debug.Log($"slot: {BoardSlot.slot}, isOverlapCard: {BoardSlot.isOverlapCard}");

                if (BoardSlot.isOverlapCard == false)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, slot.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSlot = BoardSlot;
                }
            }

            return nearestSlot;
        }
    }
}