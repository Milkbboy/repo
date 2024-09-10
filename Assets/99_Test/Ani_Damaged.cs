using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ani_Damaged : MonoBehaviour
{
    // Start is called before the first frame update

    public float bounceDuration = 0.2f;  // 바운스 애니메이션 시간
    public GameObject effectPrefab;  // 출력할 이펙트 프리팹
    public Transform effectSpawnPoint;


    public void PlaySequence()
    {
        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // 1. 바운스 애니메이션
        sequence.Append(transform.DOShakePosition(bounceDuration, strength: new Vector3(0.5f, 0, 0), vibrato: 40, randomness: 90, snapping: false, fadeOut: true));
        sequence.AppendCallback(SpawnEffect);

        // 시퀀스 실행
        sequence.Play();
    }

    void SpawnEffect()
    {
        // 이펙트 프리팹을 지정된 위치에 출력
        if (effectPrefab != null && effectSpawnPoint != null)
        {
            Instantiate(effectPrefab, effectSpawnPoint.position, Quaternion.identity);
        }
    }

}
