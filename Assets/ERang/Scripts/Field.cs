using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class Field : MonoBehaviour
    {
        public static Field Instance { get; private set; }

        public GameObject[] filedSlots;

        void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject NeareastFieldSlot(Vector3 position)
        {
            GameObject nearestSlot = null;
            float minDistance = float.MaxValue;

            foreach (GameObject slot in filedSlots)
            {
                FieldSlot fieldSlot = slot.GetComponent<FieldSlot>();

                // Debug.Log($"slot: {fieldSlot.slot}, isOverlapCard: {fieldSlot.isOverlapCard}");

                if (fieldSlot.isOverlapCard == false)
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
                foreach (GameObject slot in filedSlots)
                {
                    int slotNum = slot.GetComponent<FieldSlot>().slot;

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