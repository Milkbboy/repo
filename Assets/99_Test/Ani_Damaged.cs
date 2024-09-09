using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ani_Damaged : MonoBehaviour
{
    // Start is called before the first frame update

    public float bounceDuration = 0.2f;  // �ٿ �ִϸ��̼� �ð�
    public GameObject effectPrefab;  // ����� ����Ʈ ������
    public Transform effectSpawnPoint;


    public void PlaySequence()
    {
        // ������ ����
        Sequence sequence = DOTween.Sequence();

        // 1. �ٿ �ִϸ��̼�
        sequence.Append(transform.DOShakePosition(bounceDuration, strength: new Vector3(0.5f, 0, 0), vibrato: 40, randomness: 90, snapping: false, fadeOut: true));
        sequence.AppendCallback(SpawnEffect);

        // ������ ����
        sequence.Play();
    }

    void SpawnEffect()
    {
        // ����Ʈ �������� ������ ��ġ�� ���
        if (effectPrefab != null && effectSpawnPoint != null)
        {
            Instantiate(effectPrefab, effectSpawnPoint.position, Quaternion.identity);
        }
    }

}
