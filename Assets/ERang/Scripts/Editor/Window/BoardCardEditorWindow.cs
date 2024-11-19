using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Data;
using System.Linq;

namespace ERang
{
    public class BoardCardEditorWindow : EditorWindow
    {
        private BoardSystem boardSystem;
        private Vector2 scrollPosition;
        private int[] cardDataIds;
        private string[] cardDataNames;
        private const int elementWidth = 220;

        [MenuItem("ERang/Board Card Editor")]
        public static void ShowWindow()
        {
            GetWindow<BoardCardEditorWindow>("Base Editor");
        }

        private void OnEnable()
        {
            boardSystem = FindObjectOfType<BoardSystem>();

            List<(int, string)> cardIdNames = CardData.GetCardIdNames();
            List<(int, string)> monsterCardIdNames = MonsterCardData.GetCardIdNames();

            cardDataIds = new int[cardIdNames.Count + monsterCardIdNames.Count];
            cardDataNames = new string[cardIdNames.Count + monsterCardIdNames.Count];

            for (int i = 0; i < cardIdNames.Count; i++)
            {
                cardDataIds[i] = cardIdNames[i].Item1;
                cardDataNames[i] = cardIdNames[i].Item2;
            }

            for (int i = 0; i < monsterCardIdNames.Count; i++)
            {
                cardDataIds[i + cardIdNames.Count] = monsterCardIdNames[i].Item1;
                cardDataNames[i + cardIdNames.Count] = monsterCardIdNames[i].Item2;
            }
        }

        private void OnGUI()
        {
            if (boardSystem == null)
            {
                EditorGUILayout.HelpBox("BoardSystem을 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("보드 카드 에디터", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginHorizontal();
            DrawBoardCards(boardSystem.AllSlots);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            // 변경 사항을 적용합니다
            if (GUILayout.Button("Apply Changes"))
            {
                EditorUtility.SetDirty(boardSystem);
                // boardSystem.UpdateHandCardUI();

                foreach (BSlot bSlot in boardSystem.AllSlots)
                {
                    if (bSlot.Card == null)
                        continue;
                    
                    Debug.Log($"SlotNum: {bSlot.SlotNum}, {string.Join(", ", bSlot.Card.Abilities.Select(ability => ability.abilityId))}");
                }
            }
        }

        private void DrawBoardCards(List<BSlot> bSlots)
        {
            foreach (BSlot bSlot in bSlots)
            {
                EditorGUILayout.BeginVertical("box", GUILayout.Width(elementWidth));

                EditorGUILayout.LabelField($"SlotNum: {bSlot.SlotNum}", GUILayout.Width(elementWidth));
                EditorGUILayout.LabelField($"Index: {bSlot.Index}", GUILayout.Width(elementWidth));
                EditorGUILayout.LabelField($"IsOverlapCard: {bSlot.IsOverlapCard}", GUILayout.Width(elementWidth));

                EditorGUILayout.LabelField("SlotCardType", GUILayout.Width(elementWidth));
                bSlot.SlotCardType = (CardType)EditorGUILayout.EnumPopup(bSlot.SlotCardType, GUILayout.Width(elementWidth));

                BaseCard card = bSlot.Card;

                if (card != null)
                {
                    // CardData 선택 드롭다운 메뉴 추가
                    EditorGUILayout.LabelField("Select Card Data", GUILayout.Width(elementWidth));
                    int selectedIndex = System.Array.IndexOf(cardDataIds, card.Id);
                    int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, cardDataNames, GUILayout.Width(elementWidth));

                    if (newSelectedIndex != selectedIndex)
                    {
                        CardData selectedCardData = CardData.GetCardData(cardDataIds[newSelectedIndex]);
                        card.UpdateCardData(selectedCardData);
                    }

                    // 카드 이미지 표시
                    if (bSlot.Card.CardImage != null)
                        GUILayout.Label(bSlot.Card.CardImage, GUILayout.Width(elementWidth), GUILayout.Height(150));
                    else
                        GUILayout.Label("No Image", GUILayout.Width(elementWidth), GUILayout.Height(150));

                    // 카드 ability 표시
                    EditorGUILayout.LabelField("Card Abilities");

                    for (int i = 0; i < card.Abilities.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();

                        card.Abilities[i].abilityId = EditorGUILayout.IntField("Ability ID", card.Abilities[i].abilityId, GUILayout.Width(80));
                        EditorGUILayout.LabelField($"{AbilityData.GetAbilityData(card.Abilities[i].abilityId).abilityType}", GUILayout.Width(80));

                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            card.Abilities.RemoveAt(i);
                            i--; // Remove the current ability and adjust the index
                        }

                        EditorGUILayout.EndHorizontal();

                    }

                    // 카드 ability 추가
                    if (GUILayout.Button("Add Ability", GUILayout.Width(elementWidth)))
                    {
                        AbilitySelectorWindow.ShowWindow((selectedAbilityId, aiDataId, aiType) =>
                        {
                            Debug.Log($"Add Ability: {selectedAbilityId}");

                            AbilityData selectedAbilityData = AbilityData.GetAbilityData(selectedAbilityId);

                            CardAbility cardAbility = new()
                            {
                                whereFrom = AbilityWhereFrom.AddedEditor,
                                
                                aiType = aiType,
                                aiDataId = aiDataId,

                                abilityId = selectedAbilityId,
                                workType = selectedAbilityData.workType,
                                duration = selectedAbilityData.duration,
                                totalDuration = selectedAbilityData.duration,

                                startTurn = BattleLogic.Instance.turnCount,
                                
                                selfSlotNum = bSlot.SlotNum,
                                targetSlotNum = bSlot.SlotNum,
                            };

                            card.Abilities.Add(cardAbility);
                        });
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}