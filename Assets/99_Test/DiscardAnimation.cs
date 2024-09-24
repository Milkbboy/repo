using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DiscardAnimation : MonoBehaviour
{
    public Transform DiscardPos;  // 타겟 오브젝트의 새로운 이름: DiscardPos (버릴 위치)
    public Transform FX_Pos;  // 이펙트 출력 위치의 새로운 이름: FX_Pos
    public GameObject effectPrefab;  // 이펙트 프리팹

    private float scaleUpAmount = 1.5f;  // 오브젝트가 커질 크기 배율
    private float scaleDownAmount = 0.65f;  // 오브젝트가 작아질 크기 배율
    private float scaleDuration = 0.3f;  // 크기 변환에 걸리는 시간
    private float rotationAmount = 360f;  // 회전할 각도 (한 바퀴)
    private float moveDuration = 0.3f;  // 타겟 지점으로 이동하는 시간
    private float effectDelay = 0.01f;  // 이펙트 출력 지연 시간 (n초 후)
    private float shrinkAmount = 0.1f;  // 타겟 위치에서 줄어들 크기 배율 (X만큼)
    private float destroyDelay = 0.1f;  // 오브젝트가 c초 후에 삭제되는 시간
    private Vector3 originalScale;  // 오브젝트의 원래 크기 저장

    void Start()
    {
        originalScale = transform.localScale;  // 처음 크기 저장

    }

    public void PlaySequence()
    {
        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // 1. FX_Pos에 n초 후 이펙트 출력 (첫 번째 단계)
        sequence.AppendCallback(() => Invoke("SpawnEffect", effectDelay));

        // 2. 오브젝트가 a만큼 커졌다가 b만큼 작아지며 회전
        sequence.Append(transform.DOScale(originalScale * scaleUpAmount, scaleDuration / 2))  // a만큼 커짐
                .Join(transform.DORotate(new Vector3(0f, 0f, rotationAmount), scaleDuration, RotateMode.FastBeyond360))  // 회전
                .Append(transform.DOScale(originalScale * scaleDownAmount, scaleDuration / 2));  // b만큼 작아짐

        // 3. DiscardPos 지점으로 이동 (타겟으로 이동하는 시간 변수로 제어)
        sequence.Append(transform.DOMove(DiscardPos.position, moveDuration)
                .SetEase(Ease.InOutQuad));  // DiscardPos로 이동

        // 4. DiscardPos에 도달한 후 오브젝트 크기가 x만큼 작아짐
        sequence.Append(transform.DOScale(originalScale * shrinkAmount, scaleDuration));

        // 5. 오브젝트 삭제
        sequence.OnComplete(() => Invoke("DestroyObject", destroyDelay));  // c초 후 오브젝트 삭제

        // 시퀀스 실행
        sequence.Play();
    }

    void SpawnEffect()
    {
        // FX_Pos에 이펙트 프리팹 생성
        if (effectPrefab != null && FX_Pos != null)
        {
            Instantiate(effectPrefab, FX_Pos.position, Quaternion.identity);
        }
    }

    void DestroyObject()
    {
        // 오브젝트 삭제
        Destroy(gameObject);
    }
}
