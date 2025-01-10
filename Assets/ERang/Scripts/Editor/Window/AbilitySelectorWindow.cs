using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class AbilitySelectorWindow : EditorWindow
    {
        private static Action<int> onAbilitySelected;
        private static List<AbilityData> abilities;
        private static BSlot selfSlot;
        private Vector2 scrollPosition;
        private Dictionary<int, Dictionary<int, bool>> selectedTargetSlots = new Dictionary<int, Dictionary<int, bool>>(); // 선택된 타겟 슬롯을 저장할 딕셔너리

        public static void ShowWindow(BSlot bSlot, Action<int> onAbilitySelectedAction)
        {
            selfSlot = bSlot;
            abilities = AbilityData.abilityData_list.OrderByDescending(x => x.iconTexture != null).ToList();
            onAbilitySelected = onAbilitySelectedAction;
            GetWindow<AbilitySelectorWindow>("Ability 선택");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Ability 선택", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (AbilityData abilityData in abilities)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // 중복되는 AbilityData의 ID 색상을 변경
                GUIStyle idStyle = new GUIStyle(EditorStyles.label);
                idStyle.normal.textColor = new Color(236, 54, 54);

                EditorGUILayout.LabelField($"Ability ID: {abilityData.abilityId}", idStyle);
                EditorGUILayout.LabelField($"Ability Type: {abilityData.abilityType}");
                EditorGUILayout.LabelField($"Ability Name: {abilityData.nameDesc}");
                EditorGUILayout.LabelField($"Value: {abilityData.value}");

                if (abilityData.iconTexture != null)
                    GUILayout.Box(abilityData.iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                if (!selectedTargetSlots.ContainsKey(abilityData.abilityId))
                    selectedTargetSlots[abilityData.abilityId] = new Dictionary<int, bool>();

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

                    onAbilitySelected?.Invoke(abilityData.abilityId);
                    Close();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private List<BSlot> GetSelectableSlots(AiData aiData, BSlot selfSlot)
        {
            List<BSlot> targetSlots = new List<BSlot>();

            switch (aiData.target)
            {
                case AiDataTarget.Self:
                case AiDataTarget.Friendly:
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
                case AiDataTarget.SelectEnemy:
                    targetSlots = BoardSystem.Instance.GetOpponentSlots(selfSlot);
                    break;
                case AiDataTarget.None:
                default:
                    Debug.LogWarning($"{aiData.ai_Id} {aiData.target} - 대상이 없음");
                    break;
            }

            return targetSlots;
        }
    }
}