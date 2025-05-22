using UnityEngine;
using System;

namespace ERang
{
    public static class CardEvents
    {
        // 드래그 중인 카드 변경 이벤트
        public static event Action<GameObject> OnDraggingCardChanged;
        public static void TriggerDraggingCardChanged(GameObject card) => OnDraggingCardChanged?.Invoke(card);

        // 타겟팅 화살표 표시 이벤트
        public static event Action<bool> OnTargetingArrowVisibilityChanged;
        public static void TriggerTargetingArrowVisibilityChanged(bool isVisible) => OnTargetingArrowVisibilityChanged?.Invoke(isVisible);
    }
}