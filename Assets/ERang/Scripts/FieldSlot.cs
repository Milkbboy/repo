using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class FieldSlot : MonoBehaviour
    {
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
            // Debug.Log($"OnTriggerEnter: {other.gameObject.name}");
            if (other.gameObject.CompareTag("HandCard"))
            {
                isOverlapCard = true;
                Debug.Log($"{slot} Slot Enter HandCard. isOverlapCard: {isOverlapCard}");
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Debug.Log($"OnTriggerExit: {other.gameObject.name}");
            if (other.gameObject.CompareTag("HandCard"))
            {
                isOverlapCard = false;
                Debug.Log($"{slot} Slot Exit HandCard isOverlapCard: {isOverlapCard}");
            }
        }
    }
}