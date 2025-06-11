using UnityEngine;
using ERang.Data;
using UnityEngine.Events;

namespace ERang
{
    public class AbilityIcon : MonoBehaviour
    {
        public int AbilityId => abilityId;
        public UnityAction<int> OnIconMouseEnterAction;

        private int abilityId;
        private int duration;
        private AbilityIconUI abilityIconUI;

        void Awake()
        {
            abilityIconUI = GetComponent<AbilityIconUI>();
        }

        public void SetIcon(int abilityId)
        {
            AbilityData abilityData = AbilityData.GetAbilityData(abilityId);

            if (abilityData == null)
            {
                Debug.LogError($"{abilityData.LogText} {Utils.RedText("테이블 데이터 없음")} - AbilityIconUI: SetIcon");
                return;
            }

            this.abilityId = abilityData.abilityId;
            duration = abilityData.duration;

            AiDataType aiDataType = AiData.GetAbilityAiDataType(abilityId);

            abilityIconUI.SetIcon(abilityData.iconTexture, aiDataType);
            abilityIconUI.SetTurnCount(duration);

            Debug.Log($"{abilityData.LogText} abilityId: {this.abilityId} 아이콘 설정 완료");
        }

        public void SetTurnCount(int turnCount)
        {
            abilityIconUI.SetTurnCount(turnCount);
        }

        public void OnIconMouseEnter()
        {
            OnIconMouseEnterAction?.Invoke(abilityId);
        }

        public void OnIconMouseExit()
        {
            OnIconMouseEnterAction?.Invoke(0);
        }
    }
}