using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ani_SelectedCard : MonoBehaviour
{

    public float hoverHeight = 0.2f;
    public float animationDuration = 0.2f;
    public float scaleFactor = 1.1f;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    public bool isDrag = false;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    void OnMouseEnter()
    {
        if (isDrag)
            return;

        transform.DOMoveY(originalPosition.y + hoverHeight, animationDuration);
        transform.DOScale(originalScale * scaleFactor, animationDuration);
    }

    void OnMouseExit()
    {
        if (isDrag)
            return;

        transform.DOMoveY(originalPosition.y, animationDuration);
        transform.DOScale(originalScale, animationDuration);
    }
}

