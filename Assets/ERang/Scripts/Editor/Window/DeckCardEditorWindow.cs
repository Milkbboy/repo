using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class DeckCardEditorWindow : EditorWindow
    {
        private DeckManager deckManager;
        private Vector2 scrollPosition;
        private int[] cardDataIds;
        private string[] cardDataNames;
        private float elementWidth = 100;

        [MenuItem("ERang/Deck Card Editor")]
        public static void ShowWindow()
        {
            GetWindow<DeckCardEditorWindow>("Base Editor");
        }

        private void OnEnable()
        {
            deckManager = FindObjectOfType<DeckManager>();

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
            if (deckManager == null)
            {
                EditorGUILayout.HelpBox("DeckManager을 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("덱 카드 에디터", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("전체 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(Player.Instance?.AllCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("덱 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckManager.Data.DeckCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("핸드 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckManager.Data.HandCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("건물 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckManager.Data.BuildingCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("그레이브 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckManager.Data.GraveCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("삭제 카드", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawDeckCards(deckManager.Data.ExtinctionCards);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            // 변경 사항을 적용합니다
            if (GUILayout.Button("Apply Changes"))
            {
                EditorUtility.SetDirty(deckManager);
                // deckManager.UpdateHandCardUI();
            }
        }

        private void DrawDeckCards(IReadOnlyList<BaseCard> cards)
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
                EditorGUILayout.BeginVertical("box", GUILayout.Width(elementWidth));

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
                    // card.UpdateCardData(selectedCardData);
                }

                // 카드 이미지 표시
                GUILayout.Label(card.CardImage, GUILayout.Width(100), GUILayout.Height(123));

                // 카드 타입
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Card Type");
                EditorGUILayout.EndHorizontal();

                // EditorGUILayout.BeginHorizontal();
                // card.SetCardType((CardType)EditorGUILayout.EnumPopup(card.CardType));
                // EditorGUILayout.EndHorizontal();

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
                // 임시 리스트를 만들어서 값 입력받기
                List<int> tempIds = new List<int>(card.AiGroupIds);

                int newCount = EditorGUILayout.IntField("AiGroupIds Count", tempIds.Count);
                // 리스트 크기 조절
                while (tempIds.Count < newCount) tempIds.Add(0);
                while (tempIds.Count > newCount) tempIds.RemoveAt(tempIds.Count - 1);

                // 각 요소 입력
                for (int i = 0; i < tempIds.Count; i++)
                {
                    tempIds[i] = EditorGUILayout.IntField($"AiGroupId {i}", tempIds[i]);
                }

                // 값이 바뀌었으면 SetAiGroupIds 호출
                // if (!tempIds.SequenceEqual(card.AiGroupIds))
                // {
                //     card.SetAiGroupIds(tempIds);
                // }

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