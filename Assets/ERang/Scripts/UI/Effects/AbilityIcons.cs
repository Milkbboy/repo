using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    /// <summary>
    /// 카드 어빌리티 아이콘을 관리
    /// </summary>
    public class AbilityIcons : MonoBehaviour
    {
        public GameObject cardObject;
        public AbilityIcon abilityIconPrefab;
        public AbilityIconDescUI abilityIconDescUI;

        public List<AbilityIcon> abilityIcons = new();

        private BSlot bSlot;
        private float iconSpacing = .01f;
        private float cardWidth;
        private Vector3 startPosition;

        public void SetSlot(BSlot bSlot, GameObject cardObject)
        {
            this.cardObject = cardObject;
            this.bSlot = bSlot;

            // 아이콘 간격과 카드 크기 설정
            Transform cardFrameBackTransform = cardObject.transform.Find("Card_Frame_Back");
            MeshRenderer meshRenderer = cardFrameBackTransform.GetComponent<MeshRenderer>();

            // 카드 이미지의 Bounds 가져오기
            Bounds bounds = meshRenderer.bounds;
            cardWidth = bounds.size.x;

            // 카드 이미지의 왼쪽 하단 위치 계산
            startPosition = bounds.min;
            // Debug.Log($"{bSlot.LogText} 어빌리티 아이콘 시작 위치: {startPosition}");

            abilityIconDescUI.gameObject.SetActive(false);
        }

        public void SetIcons(List<CardAbility> cardAbilities)
        {
            List<string> lotTexts = new();

            foreach (var cardAbility in cardAbilities)
            {
                AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);

                if (abilityData == null)
                {
                    Debug.LogError($"{abilityData.LogText} {Utils.RedText("테이블 데이터 없음")} - AbilityIcons: SetIcons");
                    return;
                }

                if (string.IsNullOrEmpty(abilityData.skillIcon))
                    continue;

                AbilityIcon abilityIcon = abilityIcons.Find(x => x.AbilityId == cardAbility.abilityId);

                // 없으면 생성
                if (abilityIcon == null)
                {
                    // 아이콘 생성
                    AbilityIcon icon = Instantiate(abilityIconPrefab);
                    icon.transform.SetParent(transform, false);

                    // 부모의 스케일을 고려하여 아이콘의 스케일 설정
                    Vector3 parentScale = transform.lossyScale;
                    // 부모의 Scale 값이 2 이기 때문에 자식으로 생성되면 Scale 이 2배가 되서 확인 해보면 0.3 * 2 = 0.6 이다. 그래서 원래 사이즈로 계산
                    // 하지만 아이콘이 너무 작아 보기 힘들어 2배 사이즈 유지
                    // icon.transform.localScale = new Vector3(0.3f / parentScale.x, 0.3f / parentScale.y, 0.3f / parentScale.z);
                    icon.GetComponent<AbilityIcon>().SetIcon(cardAbility.abilityId);
                    icon.GetComponent<AbilityIcon>().OnIconMouseEnterAction += SetMouseOverAbilityIcon;

                    abilityIcons.Add(icon);

                    lotTexts.Add($"{bSlot.LogText} 어빌리티 아이콘 생성: {cardAbility.abilityId} duration: {cardAbility.duration}");
                    continue;
                }

                // duration이 0이면 아이콘 제거
                if (cardAbility.duration == 0)
                {
                    RemoveIcon(cardAbility.abilityId);
                    continue;
                }

                abilityIcon.SetTurnCount(cardAbility.duration);

                lotTexts.Add($"{bSlot.LogText} 어빌리티 아이콘 업데이트: {cardAbility.abilityId} duration: {cardAbility.duration}");
            }

            // cardAbilities 와 차이나는 아이콘 제거
            List<AbilityIcon> removeIcons = abilityIcons.Where(x => !cardAbilities.Exists(y => y.abilityId == x.AbilityId)).ToList();

            if (removeIcons.Count > 0)
                Debug.Log($"{bSlot.LogText} 차이나는 어빌리티 아이콘 제거: {removeIcons.Count} {string.Join(", ", removeIcons.Select(x => x.AbilityId))}");

            foreach (AbilityIcon removeIcon in removeIcons)
            {
                abilityIcons.Remove(removeIcon);
                Destroy(removeIcon.gameObject);
            }

            // foreach (string logText in lotTexts)
            //     Debug.Log(logText);

            UpdateIconPosition();
        }

        public void RemoveAllIcons()
        {
            foreach (AbilityIcon abilityIcon in abilityIcons)
                Destroy(abilityIcon.gameObject);

            abilityIcons.Clear();
        }

        private void RemoveIcon(int abilityId)
        {
            AbilityIcon abilityIcon = abilityIcons.Find(x => x.AbilityId == abilityId);

            if (abilityIcon == null)
            {
                Debug.LogError($"{bSlot} 어빌리티 아이콘 없음 - RemoveIcon: {abilityId}");
                return;
            }

            Debug.Log($"{bSlot} 어빌리티 아이콘 제거: {abilityId}");

            abilityIcons.Remove(abilityIcon);
            Destroy(abilityIcon.gameObject);

            UpdateIconPosition();
        }

        private void UpdateIconPosition()
        {
            // 현재 위치 설정
            Vector3 currentPosition = startPosition;

            for (int i = 0; i < abilityIcons.Count; i++)
            {
                AbilityIcon icon = abilityIcons[i];

                if (icon == null)
                {
                    Debug.LogError($"{bSlot.LogText} 어빌리티 아이콘 없음 - UpdateIconPosition");
                    continue;
                }

                // 하위 Icon 오브젝트의 Renderer
                Renderer iconRenderer = icon.GetComponentInChildren<Renderer>();

                // 아이콘 크기 설정
                float iconWidth = iconRenderer.bounds.size.x;
                float iconHeight = iconRenderer.bounds.size.y;

                // Debug.Log($"아이콘 크기: {iconWidth}, {iconHeight}");

                // 아이콘의 상단 왼쪽 모서리가 현재 위치에 맞도록 위치 조정
                icon.transform.position = currentPosition + new Vector3(iconWidth / 2, -iconHeight / 2, 0);

                // 다음 아이콘 위치 계산
                currentPosition.x += iconWidth + iconSpacing;

                // 카드 너비를 초과하면 다음 줄로 이동
                if (currentPosition.x > startPosition.x + cardWidth)
                {
                    currentPosition.x = startPosition.x;
                    currentPosition.y -= iconHeight + iconSpacing;
                }
            }
        }

        public void SetMouseOverAbilityIcon(int abilityId)
        {
            string iconDesc = Utils.GetAbilityIconText(abilityId);
            // Debug.Log($"SetMouseOverAbilityIcon: {abilityId}, iconDesc: {iconDesc}");

            abilityIconDescUI.gameObject.SetActive(abilityId != 0);
            abilityIconDescUI.SetDesc(iconDesc);
        }
    }
}