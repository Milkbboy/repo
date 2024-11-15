using UnityEditor;
using UnityEngine;
using ERang.Table;

namespace ERang
{
    public class AiDataTableWindow : EditorWindow
    {
        private AiDataTable aiDataTable;
        private SerializedObject serializedAiDataTable;
        private SerializedProperty itemsProperty;
        private Vector2 scrollPosition;

        [MenuItem("ERang/Table/AiDataTable")]
        public static void ShowWindow()
        {
            GetWindow<AiDataTableWindow>("Ai Data Table");
        }

        private void OnEnable()
        {
            // 윈도우가 열릴 때 AiDataTable을 로드합니다.
            aiDataTable = AssetDatabase.LoadAssetAtPath<AiDataTable>("Assets/ERang/Resources/TableExports/AiDataTable.asset");

            if (aiDataTable != null)
            {
                serializedAiDataTable = new SerializedObject(aiDataTable);
                itemsProperty = serializedAiDataTable.FindProperty("items");
            }
            else
            {
                Debug.LogError("AiDataTable.asset 파일을 찾을 수 없습니다.");
            }
        }

        private void OnGUI()
        {
            if (aiDataTable == null)
            {
                EditorGUILayout.HelpBox("AiDataTable.asset 파일을 찾을 수 없습니다.", MessageType.Error);

                if (GUILayout.Button("Create AiDataTable"))
                {
                    CreateAiDataTable();
                }

                return;
            }

            EditorGUILayout.LabelField("Ai Data Table", EditorStyles.boldLabel);

            if (GUILayout.Button("Save"))
            {
                SaveAiDataTable();
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // AiDataTable의 items를 표시합니다.
            EditorGUILayout.PropertyField(itemsProperty, true);

            EditorGUILayout.EndScrollView();
        }

        private void CreateAiDataTable()
        {
            aiDataTable = CreateInstance<AiDataTable>();
            AssetDatabase.CreateAsset(aiDataTable, "Assets/ERang/Resources/TableExports/AiDataTable.asset");
            serializedAiDataTable = new SerializedObject(aiDataTable);
            itemsProperty = serializedAiDataTable.FindProperty("items");
        }

        private void SaveAiDataTable()
        {
            serializedAiDataTable.ApplyModifiedProperties();
            EditorUtility.SetDirty(aiDataTable);
            AssetDatabase.SaveAssets();
        }
    }
}