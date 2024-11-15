using UnityEditor;
using UnityEngine;

namespace ERang
{
    [CustomPropertyDrawer(typeof(AbilityDataEntity))]
    public class AbilityDataEntityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // AbilityData_Id 값을 가져옵니다.
            SerializedProperty abilityIdProperty = property.FindPropertyRelative("AbilityData_Id");
            if (abilityIdProperty == null)
            {
                EditorGUI.LabelField(position, "AbilityData_Id: SerializedProperty is null");
                EditorGUI.EndProperty();
                return;
            }
            string abilityId = abilityIdProperty.intValue.ToString();

            // AbilityData_Id 값을 레이블로 사용합니다.
            label = new GUIContent($"{abilityId}");

            // 텍스트 색상을 설정하기 위한 GUIStyle을 생성합니다.
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            if (ColorUtility.TryParseHtmlString("#dd9933", out Color labelColor))
            {
                labelStyle.normal.textColor = labelColor; // 원하는 색상으로 변경
            }

            // 기본 속성 필드를 그립니다.
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label, labelStyle);
            position.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.indentLevel = 0;

            // 각 필드를 동적으로 가져와서 그립니다.
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();
            iterator.NextVisible(true); // 첫 번째 자식 속성으로 이동

            while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(fieldRect, iterator, true);
                position.y += EditorGUIUtility.singleLineHeight + 2;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 각 필드의 높이를 합산하여 반환합니다.
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();
            iterator.NextVisible(true); // 첫 번째 자식 속성으로 이동

            int fieldCount = 0;

            while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                fieldCount++;
            }

            return (fieldCount + 1) * (EditorGUIUtility.singleLineHeight + 2); // +1 for the Card_Id label
        }
    }
}