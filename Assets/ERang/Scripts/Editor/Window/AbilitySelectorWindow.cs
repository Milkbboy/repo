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
        private static Action<int, int, AiDataType> onAbilitySelected;
        private static List<AbilityData> abilities;
        private Vector2 scrollPosition;

        public static void ShowWindow(Action<int, int, AiDataType> onAbilitySelectedAction)
        {
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

                    if (GUILayout.Button("Select"))
                    {
                        onAbilitySelected?.Invoke(abilityData.abilityId, aiData.ai_Id, aiData.type);
                        Close();
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}