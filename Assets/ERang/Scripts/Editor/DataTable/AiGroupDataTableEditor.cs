using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang
{
    [CustomEditor(typeof(AiGroupDataTable))]
    public class AiGroupDataTableEditor : Editor
    {
        private SerializedProperty itemsProperty;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            itemsProperty = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("AiGroup Data Table", EditorStyles.boldLabel);

            // 인스펙터 창의 높이를 동적으로 가져옵니다.
            Rect lastRect = GUILayoutUtility.GetLastRect();
            float inspectorHeight = Screen.height - lastRect.y - 100; // 상단 레이블과 여백을 고려하여 높이 조정

            // 스크롤 뷰를 추가하여 리스트 창의 크기를 조정합니다.
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(inspectorHeight));
            
            EditorGUILayout.PropertyField(itemsProperty, new GUIContent("Items"), true);

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }
    }
}