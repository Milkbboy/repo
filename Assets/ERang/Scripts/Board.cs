using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class Board : MonoBehaviour
    {
        // Start is called before the first frame update
        public static Board Instance { get; private set; }

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
            boardWidth = boardSlots[0].GetComponent<BoxCollider>().size.x * boardSlots[0].transform.localScale.x;

            float totalWidth = (boardSlots.Length - 1) * (boardWidth + boardSpacing);
            float startX = -totalWidth / 2;

            for (int i = 0; i < boardSlots.Length; i++)
            {
                boardSlots[i].GetComponent<BoardSlot>().slot = i;
                float xPosition = startX + i * (boardWidth + boardSpacing);

                boardSlots[i].transform.position = new Vector3(xPosition, boardSlots[i].transform.position.y, boardSlots[i].transform.position.z);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject NeareastBoardSlot(Vector3 position)
        {
            GameObject nearestSlot = null;
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
                    nearestSlot = slot;
                }
            }

            return nearestSlot;
        }

        // Function to get the filedSlot GameObject at the current mouse position
        public GameObject GetFiledSlotAtMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                foreach (GameObject slot in boardSlots)
                {
                    int slotNum = slot.GetComponent<BoardSlot>().slot;

                    if (hit.transform.gameObject == slot)
                    {
                        float distance = Vector3.Distance(hit.point, slot.transform.position);
                        Debug.Log($"slotNum: {slotNum}, distance: {distance}");
                        return slot;
                    }
                }
            }

            return null;
        }
    }
}