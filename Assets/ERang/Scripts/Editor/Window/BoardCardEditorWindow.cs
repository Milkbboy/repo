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
        private int[] masterCardDataIds;
        private string[] masterCardDataNames;
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
            List<(int, string)> masterCardIdNames = MasterData.GetCardIdNames();

            cardDataIds = new int[cardIdNames.Count];
            cardDataNames = new string[cardIdNames.Count];
            masterCardDataIds = new int[masterCardIdNames.Count];
            masterCardDataNames = new string[masterCardIdNames.Count];

            for (int i = 0; i < cardIdNames.Count; i++)
            {
                cardDataIds[i] = cardIdNames[i].Item1;
                cardDataNames[i] = cardIdNames[i].Item2;
            }

            for (int i = 0; i < masterCardIdNames.Count; i++)
            {
                masterCardDataIds[i] = masterCardIdNames[i].Item1;
                masterCardDataNames[i] = masterCardIdNames[i].Item2;
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
                boardSystem.RefreshBoardSlot();

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
                EditorGUILayout.LabelField("SlotCardType");
                bSlot.SlotCardType = (CardType)EditorGUILayout.EnumPopup(bSlot.SlotCardType);

                BaseCard card = bSlot.Card;

                // CardData 선택 드롭다운 메뉴 추가
                EditorGUILayout.LabelField("Select Card Data");

                int[] ids = bSlot.SlotCardType switch
                {
                    CardType.Master => masterCardDataIds,
                    CardType.Creature => cardDataIds,
                    CardType.Monster => cardDataIds,
                    _ => cardDataIds,
                };

                string[] names = bSlot.SlotCardType switch
                {
                    CardType.Master => masterCardDataNames,
                    CardType.Creature => cardDataNames,
                    CardType.Monster => cardDataNames,
                    _ => cardDataNames,
                };

                int selectedIndex = System.Array.IndexOf(ids, card?.Id ?? -1);
                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, names);

                if (newSelectedIndex != selectedIndex)
                {
                    int cardId = bSlot.SlotCardType switch
                    {
                        CardType.Master => masterCardDataIds[newSelectedIndex],
                        CardType.Creature => cardDataIds[newSelectedIndex],
                        CardType.Monster => cardDataIds[newSelectedIndex],
                        _ => cardDataIds[newSelectedIndex],
                    };

                    bool inUse = false;
                    Texture2D cardTexture = null;

                    switch (bSlot.SlotCardType)
                    {
                        case CardType.Master:
                            MasterData selectedMasterCardData = MasterData.GetMasterData(cardId);
                            inUse = false;
                            cardTexture = selectedMasterCardData.GetMasterTexture();
                            card ??= new MasterCard();
                            break;
                        case CardType.Monster:
                            CardData selectedMonsterCardData = CardData.GetCardData(cardId);
                            inUse = selectedMonsterCardData.inUse;
                            cardTexture = selectedMonsterCardData.GetCardTexture();
                            card ??= new CreatureCard(selectedMonsterCardData);
                            break;
                        case CardType.Creature:
                            CardData selectedCardData = CardData.GetCardData(cardId);
                            inUse = selectedCardData.inUse;
                            cardTexture = selectedCardData.GetCardTexture();
                            card ??= new CreatureCard(selectedCardData);
                            break;
                    }

                    card.UpdateCardData(cardId, bSlot.SlotCardType, inUse, 0, cardTexture);
                    bSlot.EquipCard(card);
                }

                if (card != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    // 카드 이미지 표시
                    if (bSlot.Card.CardImage != null)
                        GUILayout.Label(bSlot.Card.CardImage, GUILayout.Width(elementWidth), GUILayout.Height(150));
                    else
                        GUILayout.Label("No Image", GUILayout.Width(elementWidth), GUILayout.Height(150));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        bSlot.RemoveCard();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

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
                        AbilitySelectorWindow.ShowWindow(bSlot, (selectedAbilityId, aiDataId, aiType, targetSlots) =>
                        {
                            Debug.Log($"Add Ability: {selectedAbilityId}, {string.Join(", ", targetSlots.Select(slot => slot.SlotNum))}");

                            AbilityData selectedAbilityData = AbilityData.GetAbilityData(selectedAbilityId);

                            AbilityLogic.Instance.AbilityAction(aiDataId, selectedAbilityId, bSlot, targetSlots, AbilityWhereFrom.AddedEditor);
                        });
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}