using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public class BoardLogic : MonoBehaviour
    {
        public static BoardLogic Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        public IEnumerator FireMissile(BSlot selfSlot, List<BSlot> targetSlots, int ackCount, int damage)
        {
            Vector3 startPosition = selfSlot.transform.position;
            List<(GameObject, Vector3)> missiles = new List<(GameObject, Vector3)>();

            foreach (BSlot targetSlot in targetSlots)
            {
                Vector3 targetPosition = targetSlot.transform.position;

                GameObject missile = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 임시로 구체를 미사일로 사용
                missile.transform.position = startPosition;
                missile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // 미사일 크기 조정

                missiles.Add((missile, targetPosition));
            }

            float duration = .2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                foreach (var missile in missiles)
                    missile.Item1.transform.position = CalculateParabolicPath(startPosition, missile.Item2, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            foreach (var missile in missiles)
                Destroy(missile.Item1);
        }

        private Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float t)
        {
            float height = 5.0f; // 포물선의 높이
            float parabola = 4 * height * t * (1 - t);
            Vector3 midPoint = Vector3.Lerp(start, end, t);
            return new Vector3(midPoint.x, midPoint.y + parabola, midPoint.z);
        }
    }
}