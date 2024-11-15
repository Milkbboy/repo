using UnityEditor;
using UnityEngine;
using ERang.Table;

namespace ERang
{
    public class CardDataTableWindow : EditorWindow
    {
        private CardDataTable cardDataTable;
        private SerializedObject serializedCardDataTable;
        private SerializedProperty itemsProperty;
        private Vector2 scrollPosition;

        [MenuItem("ERang/Table/CardDataTable")]
        public static void ShowWindow()
        {
            GetWindow<CardDataTableWindow>("Card Data Table");
        }

        private void OnEnable()
        {
            // 윈도우가 열릴 때 CardDataTable을 로드합니다.
            cardDataTable = AssetDatabase.LoadAssetAtPath<CardDataTable>("Assets/ERang/Resources/TableExports/CardDataTable.asset");

            if (cardDataTable != null)
            {
                serializedCardDataTable = new SerializedObject(cardDataTable);
                itemsProperty = serializedCardDataTable.FindProperty("items");
            }
            else
            {
                Debug.LogError("CardDataTable.asset 파일을 찾을 수 없습니다.");
            }
        }

        private void OnGUI()
        {
            if (cardDataTable == null)
            {
                EditorGUILayout.HelpBox("CardDataTable.asset 파일을 찾을 수 없습니다.", MessageType.Error);

                if (GUILayout.Button("Create CardDataTable"))
                {
                    CreateCardDataTable();
                }

                return;
            }

            EditorGUILayout.LabelField("Card Data Table", EditorStyles.boldLabel);

            if (GUILayout.Button("Save"))
            {
                SaveCardDataTable();
            }

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (serializedCardDataTable != null)
            {
                serializedCardDataTable.Update();
                if (itemsProperty != null)
                {
                    EditorGUILayout.PropertyField(itemsProperty, new GUIContent("Card Data Items"), true);
                }
                serializedCardDataTable.ApplyModifiedProperties();
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateCardDataTable()
        {
            cardDataTable = ScriptableObject.CreateInstance<CardDataTable>();
            AssetDatabase.CreateAsset(cardDataTable, "Assets/ERang/Resources/TableExports/CardDataTable.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cardDataTable;
        }

        private void SaveCardDataTable()
        {
            EditorUtility.SetDirty(cardDataTable);
            AssetDatabase.SaveAssets();
        }
    }
}