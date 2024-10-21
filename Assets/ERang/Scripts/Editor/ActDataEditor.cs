using UnityEditor;
using ERang.Data;

namespace ERang
{
    [CustomEditor(typeof(ActData))]
    public class ActDataEditor : Editor
    {
        private SerializedProperty actID;
        private SerializedProperty nameDesc;
        private SerializedProperty areaIds;
        private SerializedProperty eventIds;

        private void OnEnable()
        {
            actID = serializedObject.FindProperty("actID");
            nameDesc = serializedObject.FindProperty("nameDesc");
            areaIds = serializedObject.FindProperty("areaIds");
            eventIds = serializedObject.FindProperty("eventIds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(actID);
            EditorGUILayout.PropertyField(nameDesc);
            EditorGUILayout.PropertyField(areaIds, true);
            EditorGUILayout.PropertyField(eventIds, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Act Datas", EditorStyles.boldLabel);

            foreach (var actData in ActData.actDatas)
            {
                EditorGUILayout.LabelField($"Act ID: {actData.actID}, Name: {actData.nameDesc}");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}