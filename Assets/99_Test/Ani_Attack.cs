using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ani_Attack : MonoBehaviour
{
    public Transform originPoint;  // �ʱ� ��ġ�� ������ ����
    public float moveBackward = -0.3f;  // �������� �̵��� �Ÿ�
    public float moveForward = 1.5f;  // ���������� �̵��� �Ÿ�
    public float moveDuration = 0.1f;  // �̵��ϴ� �ð�
    public float scaleMultiplier = 1.3f;  // �������� Ŀ���� ����
    public float scaleDuration = 0.1f;  // ũ�� ��ȭ �ð�

    private Vector3 originalPosition;  // originPoint �� ��ġ�� ���� ��ġ�� ���
    private Vector3 originalScale;     // ���� ũ�� ����
    public bool isAttackingFromLeft = true;  // ���ʿ��� �����ϴ��� ���θ� �����ϴ� ����

    void Start()
    {
        // originPoint �� ��ġ�� ���� ��ġ�� ���� 
        if (originPoint != null)
        {
            // ������Ʈ�� ��ġ�� Ư�� ������Ʈ�� ��ġ�� ���� (����)
            transform.position = originPoint.position;
            originalPosition = originPoint.position;  // originPoint�� ��ġ�� ���� ��ġ�� ����
        }
        else
        {
            originalPosition = transform.position;  // ������ ������Ʈ�� ������ ���� ��ġ�� ���
        }

        originalScale = transform.localScale;   // ó�� ũ�� ����
    }

    public void PlaySequence()
    {
        // ������ ����
        Sequence sequence = DOTween.Sequence();

        // 1. �������� z��ŭ Ŀ���� �ִϸ��̼�
        sequence.Append(transform.DOScale(originalScale * scaleMultiplier, scaleDuration));

        // 2. ���� ���⿡ ���� X�� �̵� �Ÿ� ����
        float directionMultiplier = isAttackingFromLeft ? 1 : -1;  // ���ʿ��� �����ϸ� -1, �����ʿ��� �����ϸ� 1

        // 3. �������� �������� X��ŭ �̵�
        sequence.Append(transform.DOMoveX(originalPosition.x + moveBackward * directionMultiplier, moveDuration));

        // 4. �������� ���������� Y��ŭ �̵�
        sequence.Append(transform.DOMoveX(originalPosition.x + moveForward * directionMultiplier, moveDuration));

        // 5. ���� ��ġ�� ���ƿ�
        sequence.Append(transform.DOMove(originalPosition, moveDuration));

        // 6. ������ ũ�⸦ ������� �ǵ����� �ִϸ��̼�
        sequence.Append(transform.DOScale(originalScale, scaleDuration));

        // ������ ����
        sequence.Play();
    }
}
