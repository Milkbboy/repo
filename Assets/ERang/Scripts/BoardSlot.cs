using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BoardSlot : MonoBehaviour
    {
        // Start is called before the first frame update
        public int slot;
        // 현재 사용 중인지 여부
        public bool isOccupied = false;
        // 카드가 올라가 있는지 여부
        public bool isOverlapCard = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("HandCard"))
            {
                isOverlapCard = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("HandCard"))
            {
                isOverlapCard = false;
            }
        }
    }
}