using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Data;

namespace ERang
{
    public class DeckCardEditorWindow : EditorWindow
    {
        private DeckSystem deckSystem;
        private Vector2 scrollPosition;
        private int[] cardDataIds;
        private string[] cardDataNames;

        [MenuItem("ERang/Deck Card Editor")]
        public static void ShowWindow()
        {
            GetWindow<DeckCardEditorWindow>("Base Editor");
        }

        private void OnEnable()
        {
            deckSystem = FindObjectOfType<DeckSystem>();

            List<(int, string)> cardIdNames = CardData.GetCardIdNames();

            cardDataIds = new int[cardIdNames.Count];
            cardDataNames = new string[cardIdNames.Count];

            for (int i = 0; i < cardIdNames.Count; i++)
            {
                cardDataIds[i] = cardIdNames[i].Item1;
                cardDataNames[i] = cardIdNames[i].Item2;
            }
        }

        private void OnGUI()
        {
            if (deckSystem == null)
            {
                EditorGUILayout.HelpBox("DeckSystem을 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("덱 카드 에디터", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("덱 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckSystem.DeckCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("핸드 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckSystem.HandCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("건물 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckSystem.BuildingCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("그레이브 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckSystem.GraveCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("삭제 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckSystem.ExtinctionCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            // 변경 사항을 적용합니다
            if (GUILayout.Button("Apply Changes"))
            {
                EditorUtility.SetDirty(deckSystem);
                deckSystem.UpdateHandCardUI();
            }
        }

        private void DrawDeckCards(List<BaseCard> cards)
        {
            // 카드가 없으면 표시
            if (cards.Count == 0)
            {
                EditorGUILayout.HelpBox("카드가 없습니다.", MessageType.Info);
                return;
            }

            foreach (var card in cards)
            {
                // 수직 레이아웃 시작
                EditorGUILayout.BeginVertical("box", GUILayout.Width(80));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Card Id: {card.Id}");
                EditorGUILayout.EndHorizontal();

                // CardData 선택 드롭다운 메뉴 추가
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Card Data");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                int selectedIndex = System.Array.IndexOf(cardDataIds, card.Id);
                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, cardDataNames);
                EditorGUILayout.EndHorizontal();

                if (newSelectedIndex != selectedIndex)
                {
                    CardData selectedCardData = CardData.GetCardData(cardDataIds[newSelectedIndex]);
                    card.UpdateCardData(selectedCardData);
                }

                // 카드 이미지 표시
                if (card.CardImage != null)
                    GUILayout.Label(card.CardImage, GUILayout.Width(100), GUILayout.Height(150));
                else
                    GUILayout.Label("No Image", GUILayout.Width(100), GUILayout.Height(150));

                // 카드 타입
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Card Type");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                card.CardType = (CardType)EditorGUILayout.EnumPopup(card.CardType);
                EditorGUILayout.EndHorizontal();

                // 카드 클래스 별로 스탯 표시
                if (card is CreatureCard creatureCard)
                {
                    CreatureCard(creatureCard);
                }
                else if (card is MagicCard magicCard)
                {
                    MagicCard(magicCard);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("AiGroupId");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                card.AiGroupId = EditorGUILayout.IntField(card.AiGroupId);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private HCard FindHCardByCardUid(string cardUid)
        {
            HCard[] hCards = FindObjectsOfType<HCard>();

            foreach (HCard hCard in hCards)
            {
                if (hCard.Card.Uid == cardUid)
                {
                    return hCard;
                }
            }
            return null;
        }

        private void CreatureCard(CreatureCard card)
        {
            int hp = card.Hp;
            int mana = card.Mana;
            int atk = card.Atk;
            int def = card.Def;

            DrawCardStatField("Hp", ref hp);
            DrawCardStatField("Mana", ref mana);
            DrawCardStatField("Atk", ref atk);
            DrawCardStatField("Def", ref def);

            card.SetHp(hp);
            card.SetMana(mana);
            card.SetAttack(atk);
            card.SetDefense(def);
        }

        private void MagicCard(MagicCard card)
        {
            int mana = card.Mana;
            int atk = card.Atk;

            DrawCardStatField("Mana", ref mana);
            DrawCardStatField("Atk", ref atk);

            card.SetMana(mana);
            card.SetAttack(atk);
        }

        private void DrawCardStatField(string label, ref int stat)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            stat = EditorGUILayout.IntField(stat);
            EditorGUILayout.EndHorizontal();
        }
    }
}