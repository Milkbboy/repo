using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ani_Attack : MonoBehaviour
{
    public Transform originPoint;  // 초기 위치를 참조할 변수
    public float moveBackward = -0.3f;  // 왼쪽으로 이동할 거리
    public float moveForward = 1.5f;  // 오른쪽으로 이동할 거리
    public float moveDuration = 0.1f;  // 이동하는 시간
    public float scaleMultiplier = 1.3f;  // 프리펩이 커지는 배율
    public float scaleDuration = 0.1f;  // 크기 변화 시간

    private Vector3 originalPosition;  // originPoint 의 위치를 원래 위치로 사용
    private Vector3 originalScale;     // 원래 크기 저장
    public bool isAttackingFromLeft = true;  // 왼쪽에서 공격하는지 여부를 결정하는 변수

    void Start()
    {
        // originPoint 의 위치를 스폰 위치로 설정 
        if (originPoint != null)
        {
            // 오브젝트의 위치를 특정 오브젝트의 위치로 설정 (스폰)
            transform.position = originPoint.position;
            originalPosition = originPoint.position;  // originPoint의 위치를 원래 위치로 설정
        }
        else
        {
            originalPosition = transform.position;  // 참조할 오브젝트가 없으면 현재 위치를 사용
        }

        originalScale = transform.localScale;   // 처음 크기 저장
    }

    public void PlaySequence()
    {
        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // 1. 프리펩이 z만큼 커지는 애니메이션
        sequence.Append(transform.DOScale(originalScale * scaleMultiplier, scaleDuration));

        // 2. 공격 방향에 따른 X축 이동 거리 설정
        float directionMultiplier = isAttackingFromLeft ? 1 : -1;  // 왼쪽에서 공격하면 -1, 오른쪽에서 공격하면 1

        // 3. 프리펩을 왼쪽으로 X만큼 이동
        sequence.Append(transform.DOMoveX(originalPosition.x + moveBackward * directionMultiplier, moveDuration));

        // 4. 프리펩을 오른쪽으로 Y만큼 이동
        sequence.Append(transform.DOMoveX(originalPosition.x + moveForward * directionMultiplier, moveDuration));

        // 5. 원래 위치로 돌아옴
        sequence.Append(transform.DOMove(originalPosition, moveDuration));

        // 6. 프리펩 크기를 원래대로 되돌리는 애니메이션
        sequence.Append(transform.DOScale(originalScale, scaleDuration));

        // 시퀀스 실행
        sequence.Play();
    }
}
