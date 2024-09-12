using UnityEngine;
using TMPro;
using ERang.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ERang
{
    public class CardUI : MonoBehaviour
    {
        public Image cardImage;
        public MeshRenderer cardMeshRenderer;
        public TextMeshProUGUI cardTypeText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI manaText;
        public TextMeshProUGUI atkText;
        public TextMeshProUGUI defText;
        public GameObject cardObject;
        public TextMeshProUGUI floatingTextPrefab;
        public Transform floatingTextParent;

        private Texture2D originTexture;
        private List<TextMeshProUGUI> floatingTextList = new List<TextMeshProUGUI>();

        void Awake()
        {
        }

        void Start()
        {
        }

        public void SetCard(Card card)
        {
            // Debug.Log("CardUI SetCard: " + cardId);
            CardData cardData = (card.Type == CardType.Monster) ? MonsterCardData.GetCardData(card.Id) : CardData.GetCardData(card.Id);
            Texture2D cardTexture = cardData.GetCardTexture();

            if (!cardTexture)
            {
                Debug.LogError($"${cardData.card_id} Card texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                // 원래 텍스쳐 저장
                if (originTexture == null)
                {
                    originTexture = (Texture2D)cardMeshRenderer.materials[0].GetTexture("_BaseMap");
                }

                cardMeshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
            }
            else
            {
                cardImage.sprite = Sprite.Create(cardTexture, new Rect(0, 0, cardTexture.width, cardTexture.height), Vector2.zero);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = card.Type.ToString();
            }

            // 카드 정보 표시
            if (descText != null)
            {
                descText.text = $"gold: {card.costGold.ToString()}\nmana: {card.costMana.ToString()}";

                if (card.hp > 0)
                {
                    descText.text += $"\nhp: {card.hp}";
                }

                if (card.atk > 0)
                {
                    descText.text += $"\natk: {card.atk}";
                }
            }

            hpText.text = card.hp.ToString();
            manaText.text = card.costMana.ToString();
            atkText.text = card.atk.ToString();
            defText.text = card.def.ToString();
        }

        public void SetMasterCard(Master master)
        {
            // Debug.Log("CardUI SetMasterCard: " + master.masterId);
            MasterData masterData = MasterData.master_dict[master.MasterId];

            // Debug.Log("CardUI SetCard: " + cardId);
            Texture2D cardTexture = masterData.GetMasterTexture();

            if (!cardTexture)
            {
                Debug.LogError($"${master.MasterId} Master texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
            }
            else
            {
                cardImage.sprite = Sprite.Create(cardTexture, new Rect(0, 0, cardTexture.width, cardTexture.height), Vector2.zero);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = "Master";
            }

            SetMasterStat(master.Hp, master.MaxHp, master.Atk, master.Def, master.Mana, master.MaxMana);
        }

        public void SetEnemyMasterCard(Enemy enemy)
        {
            // Debug.Log("CardUI SetEnemyMasterCard: " + enemy.enemyId);
            MasterData enemyData = MasterData.master_dict[enemy.enemyId];

            // Debug.Log("CardUI SetCard: " + cardId);
            Texture2D enemyTexture = enemyData.GetMasterTexture();

            if (!enemyTexture)
            {
                Debug.LogError($"${enemy.enemyId} Enemy Master texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", enemyTexture);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = "Enemy Master";
            }

            SetMasterStat(enemy.Hp, enemy.MaxHp, enemy.Atk, enemy.Def);
        }

        public void SetMasterStat(int hp, int maxHp, int atk, int def, int mana = 0, int maxMana = 0)
        {
            if (descText != null)
            {
                descText.text = $"hp: {hp}/{maxHp}\nmana: {mana}/{maxMana}\natk: {atk}\ndef: {def}";
            }

            hpText.text = $"{hp}";
            manaText.text = $"{mana}";
            atkText.text = atk.ToString();
            defText.text = def.ToString();
        }

        public void SetMasterMana(int mana)
        {
            ShowFloatingText("mana", manaText.text, mana.ToString());
            manaText.text = mana.ToString();
        }

        public void SetHp(int hp)
        {
            ShowFloatingText("hp", hpText.text, hp.ToString());
            hpText.text = hp.ToString();
        }

        public void SetAtk(int atk)
        {
            if (atk < 0)
                atk = 0;

            ShowFloatingText("Atk", atkText.text, atk.ToString());
            atkText.text = atk.ToString();
        }

        public void SetDef(int def)
        {
            if (def < 0)
                def = 0;

            ShowFloatingText("Def", defText.text, def.ToString());
            defText.text = def.ToString();
        }

        public void SetGold(int beforeGlod, int afterGold)
        {
            ShowFloatingText("Gold", beforeGlod.ToString(), afterGold.ToString());
        }

        public void ResetStat()
        {
            hpText.text = string.Empty;
            manaText.text = string.Empty;
            atkText.text = string.Empty;
            defText.text = string.Empty;

            // 원래 텍스쳐로 복구
            if (originTexture != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", originTexture);
            }
        }

        private void ShowFloatingText(string text, string oldValue, string newValue)
        {
            // Debug.Log($"ShowFloatingText: {stat}, {oldValue}, {newValue}");

            if (floatingTextPrefab == null || floatingTextParent == null)
            {
                Debug.LogWarning("FloatingTextPrefab or FloatingTextParent is null");
                return;
            }

            int oldValueInt = int.Parse(oldValue);
            int newValueInt = int.Parse(newValue);

            int diff = newValueInt - oldValueInt;

            if (diff == 0) return;

            string diffText = diff > 0 ? $"<color=green>+{diff}</color>" : $"<color=red>{diff.ToString()}</color>";
            TextMeshProUGUI floatingText = Instantiate(floatingTextPrefab, floatingTextParent);
            floatingText.text = $"{text} {diffText}";

            RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition += new Vector2(0, floatingTextList.Count * 25f);

            floatingTextList.Add(floatingText);

            StartCoroutine(BlinkFloatingText(floatingText));
        }

        private IEnumerator ScrollFloatingText(TextMeshProUGUI floatingText)
        {
            Vector3 startPos = floatingText.transform.position; // 시작 위치
            Vector3 endPos = startPos + new Vector3(0, 50, 0);
            float duration = 4f; // 지속시간
            float elapsed = 0; // 경과 시간

            while (elapsed < duration)
            {
                floatingText.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            floatingTextList.Remove(floatingText);
            Destroy(floatingText.gameObject);
        }

        private IEnumerator BlinkFloatingText(TextMeshProUGUI floatingText)
        {
            float duration = 3f; // 지속시간
            float blinkInterval = 0.5f; // 깜빡이는 간격
            float elapsed = 0; // 경과 시간

            while (elapsed < duration)
            {
                // 투명도 변경
                Color color = floatingText.color;
                color.a = color.a == 1 ? 0 : 1;
                floatingText.color = color;

                elapsed += blinkInterval;
                yield return new WaitForSeconds(blinkInterval);
            }

            floatingTextList.Remove(floatingText);
            Destroy(floatingText.gameObject);
        }
    }
}