using System.Linq;
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
            EditorGUILayout.LabelField("Deck Cards", EditorStyles.boldLabel);

            if (deckSystem.DeckCards == null || deckSystem.DeckCards.Count == 0)
            {
                EditorGUILayout.LabelField("No cards in the deck.");
                return;
            }

            foreach (BaseCard card in deckSystem.DeckCards)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Uid", card.Uid);
                EditorGUILayout.LabelField("Id", card.Id.ToString());
                EditorGUILayout.LabelField("CardType", card.CardType.ToString());
                EditorGUILayout.LabelField("AiGroupId", string.Join(",", card.AiGroupIds));
                EditorGUILayout.LabelField("AiGroupIndex", string.Join(", ", card.AiGroupIndexes.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
                EditorGUILayout.LabelField("InUse", card.InUse.ToString());
                EditorGUILayout.LabelField("IsExtinction", card.IsExtinction.ToString());
                EditorGUILayout.LabelField("Hp", card.Hp.ToString());
                EditorGUILayout.LabelField("Atk", card.Atk.ToString());
                EditorGUILayout.LabelField("Def", card.Def.ToString());
                EditorGUILayout.LabelField("Mana", card.Mana.ToString());
                EditorGUILayout.LabelField("Traits", card.Traits.ToString());
                EditorGUILayout.LabelField("HandAbility Count", card.AbilitySystem.HandAbilities.Count.ToString());

                // 카드 이미지 표시
                if (card.CardImage != null)
                {
                    GUILayout.Label(card.CardImage, GUILayout.Width(100), GUILayout.Height(123));
                }

                // 카드 능력 표시
                EditorGUILayout.LabelField("Card HandAbilities", EditorStyles.boldLabel);
                foreach (var ability in card.AbilitySystem.HandAbilities)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("abilityId", ability.abilityId.ToString());
                    EditorGUILayout.LabelField("aiDataId", ability.aiDataId.ToString());
                    EditorGUILayout.LabelField("aiType", ability.aiType.ToString());
                    EditorGUILayout.LabelField("abilityType", ability.abilityType.ToString());
                    EditorGUILayout.LabelField("abilityValue", ability.abilityValue.ToString());
                    EditorGUILayout.LabelField("workType", ability.workType.ToString());
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("All Cards", EditorStyles.boldLabel);

            if (Player.Instance == null)
                return;

            if (Player.Instance.AllCards.Count == 0)
                return;

            foreach (BaseCard card in Player.Instance.AllCards)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Uid", card.Uid);
                EditorGUILayout.LabelField("Id", card.Id.ToString());
                EditorGUILayout.LabelField("CardType", card.CardType.ToString());
                EditorGUILayout.LabelField("AiGroupId", string.Join(",", card.AiGroupIds));
                EditorGUILayout.LabelField("AiGroupIndex", string.Join(", ", card.AiGroupIndexes.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
                EditorGUILayout.LabelField("InUse", card.InUse.ToString());
                EditorGUILayout.LabelField("IsExtinction", card.IsExtinction.ToString());
                EditorGUILayout.LabelField("abilityCount", card.AbilitySystem.CardAbilities.Count.ToString());
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