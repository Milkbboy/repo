using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ERang
{
    [CustomEditor(typeof(DeckSystem))]
    public class DeckSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DeckSystem deckSystem = (DeckSystem)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("All Cards", EditorStyles.boldLabel);

            if (Player.Instance.AllCards.Count == 0)
                return;

            foreach (BaseCard card in Player.Instance.AllCards)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Uid", card.Uid);
                EditorGUILayout.LabelField("Id", card.Id.ToString());
                EditorGUILayout.LabelField("CardType", card.CardType.ToString());
                EditorGUILayout.LabelField("AiGroupId", card.AiGroupId.ToString());
                EditorGUILayout.LabelField("AiGroupIndex", card.AiGroupIndex.ToString());
                EditorGUILayout.LabelField("InUse", card.InUse.ToString());
                EditorGUILayout.LabelField("IsExtinction", card.IsExtinction.ToString());
                EditorGUILayout.LabelField("abilityCount", card.Abilities.Count.ToString());
                GUILayout.Label(card.CardImage, GUILayout.Width(100), GUILayout.Height(123));

                // 카드의 실제 타입에 따라 속성을 표시
                EditorGUILayout.LabelField("Hp", card.Hp.ToString());
                EditorGUILayout.LabelField("Atk", card.Atk.ToString());
                EditorGUILayout.LabelField("Def", card.Def.ToString());
                EditorGUILayout.LabelField("Mana", card.Mana.ToString());
                EditorGUILayout.EndVertical();
            }
        }
    }
}