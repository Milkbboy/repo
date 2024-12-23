using UnityEngine;

namespace ERang
{
    public class AbilityEventHandler : MonoBehaviour
    {
        private AbilityIcon abilityIcon;

        void Awake()
        {
            abilityIcon = GetComponentInParent<AbilityIcon>();

            if (abilityIcon == null)
                Debug.LogError($"{Utils.RedText("AbilityIcon 컴포넌트를 찾을 수 없습니다.")} - AbilityEventHandler: Awake");
        }

        public void OnMouseEnter()
        {
            abilityIcon?.OnIconMouseEnter();
        }

        public void OnMouseExit()
        {
            abilityIcon?.OnIconMouseExit();
        }
    }
}