using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using ERang.Data;

namespace ERang
{
    public class AbilitySelectorWindow : EditorWindow
    {
        private static Action<int, int, AiDataType, List<BSlot>> onAbilitySelected;
        private static List<AbilityData> abilities;
        private static BSlot selfSlot;
        private Vector2 scrollPosition;
        private Dictionary<int, Dictionary<int, bool>> selectedTargetSlots = new Dictionary<int, Dictionary<int, bool>>(); // 선택된 타겟 슬롯을 저장할 딕셔너리

        public static void ShowWindow(BSlot bSlot, Action<int, int, AiDataType, List<BSlot>> onAbilitySelectedAction)
        {
            selfSlot = bSlot;
            abilities = AbilityData.abilityData_list;
            onAbilitySelected = onAbilitySelectedAction;
            GetWindow<AbilitySelectorWindow>("Ability 선택");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Ability 선택", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            HashSet<int> displayedAbilityIds = new HashSet<int>();

            foreach (AbilityData abilityData in abilities)
            {
                // AiData와 연결된 AbilityData 를 찾기
                List<AiData> connectedAiDatas = AiData.ai_list.FindAll(aiData => aiData.ability_Ids.Contains(abilityData.abilityId));

                // AbilityData와 연결된 AiData 정보를 함께 표시
                foreach (AiData aiData in connectedAiDatas)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // 중복되는 AbilityData의 ID 색상을 변경
                    GUIStyle idStyle = new GUIStyle(EditorStyles.label);
                    if (displayedAbilityIds.Contains(abilityData.abilityId))
                    {
                        idStyle.normal.textColor = new Color(236, 54, 54);
                    }
                    else
                    {
                        displayedAbilityIds.Add(abilityData.abilityId);
                    }

                    EditorGUILayout.LabelField($"Ability ID: {abilityData.abilityId}", idStyle);
                    EditorGUILayout.LabelField($"Ability Type: {abilityData.abilityType}");
                    EditorGUILayout.LabelField($"AiData ID: {aiData.ai_Id}");
                    EditorGUILayout.LabelField($"AiData Type: {aiData.type}");
                    EditorGUILayout.LabelField($"AiData Target: {aiData.target}");
                    

                    // 타겟 보드 슬롯 선택 드롭다운 메뉴
                    List<BSlot> targetSlots = GetSelectableSlots(aiData, selfSlot);

                    if (!selectedTargetSlots.ContainsKey(abilityData.abilityId))
                        selectedTargetSlots[abilityData.abilityId] = new Dictionary<int, bool>();

                    EditorGUILayout.BeginVertical(); // 수직 레이아웃 시작
                    foreach (BSlot slot in targetSlots)
                    {
                        if (!selectedTargetSlots[abilityData.abilityId].ContainsKey(slot.SlotNum))
                        {
                            selectedTargetSlots[abilityData.abilityId][slot.SlotNum] = false;
                        }

                        EditorGUILayout.BeginHorizontal(); // 수평 레이아웃 시작
                        selectedTargetSlots[abilityData.abilityId][slot.SlotNum] = EditorGUILayout.Toggle(selectedTargetSlots[abilityData.abilityId][slot.SlotNum], GUILayout.Width(20));
                        EditorGUILayout.LabelField($"Slot {slot.SlotNum}", GUILayout.Width(60));
                        EditorGUILayout.EndHorizontal(); // 수평 레이아웃 종료
                    }
                    EditorGUILayout.EndVertical(); // 수직 레이아웃 종료

                    if (GUILayout.Button("Select"))
                    {
                        List<int> selectedSlots = new List<int>();
                        foreach (var kvp in selectedTargetSlots[abilityData.abilityId])
                        {
                            if (kvp.Value)
                            {
                                selectedSlots.Add(kvp.Key);
                            }
                        }

                        onAbilitySelected?.Invoke(abilityData.abilityId, aiData.ai_Id, aiData.type, targetSlots);
                        Close();
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private List<BSlot> GetSelectableSlots(AiData aiData, BSlot selfSlot)
        {
            List<BSlot> targetSlots = new List<BSlot>();

            switch (aiData.target)
            {
                case AiDataTarget.Self:
                case AiDataTarget.AllFriendly:
                case AiDataTarget.AllFriendlyCreature:
                    targetSlots = BoardSystem.Instance.GetFriendlySlots(selfSlot);
                    break;
                case AiDataTarget.Enemy:
                case AiDataTarget.NearEnemy:
                case AiDataTarget.AllEnemy:
                case AiDataTarget.AllEnemyCreature:
                case AiDataTarget.RandomEnemy:
                case AiDataTarget.RandomEnemyCreature:
                case AiDataTarget.FirstEnemy:
                case AiDataTarget.SecondEnemy:
                    targetSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);
                    break;
                case AiDataTarget.None:
                default:
                    Debug.LogWarning($"{aiData.ai_Id} - 대상이 없음");
                    break;
            }

            return targetSlots;
        }
    }
}