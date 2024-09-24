using UnityEditor;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    [CustomEditor(typeof(LevelGroupData))]
    public class LevelGroupDataEditor : Editor
    {
        private SerializedProperty levelGroupID;
        private SerializedProperty levelDatas;

        private void OnEnable()
        {
            levelGroupID = serializedObject.FindProperty("levelGroupID");
            levelDatas = serializedObject.FindProperty("levelDatas");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(levelGroupID);

            EditorGUILayout.LabelField("Level Datas", EditorStyles.boldLabel);
            for (int i = 0; i < levelDatas.arraySize; i++)
            {
                SerializedProperty levelData = levelDatas.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(levelData, new GUIContent($"Level Data {i + 1}"), true);
            }

            if (GUILayout.Button("Add Level Data"))
            {
                levelDatas.InsertArrayElementAtIndex(levelDatas.arraySize);
            }

            if (GUILayout.Button("Remove Last Level Data"))
            {
                if (levelDatas.arraySize > 0)
                {
                    levelDatas.DeleteArrayElementAtIndex(levelDatas.arraySize - 1);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}