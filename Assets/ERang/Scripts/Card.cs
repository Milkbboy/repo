using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int repositoryPosition;
    public bool selected;

    public int handPosition, deckPosition, fieldPosition;
    private Vector3 originalPosition; // 카드의 원래 Z 위치

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!selected)
        {
            MoveToPosition();
        }

        if (Input.GetMouseButtonUp(0))
        {
            selected = false;
        }
    }

    void MoveToPosition()
    {
        Vector3 toPosition = new Vector3(0, 0, 0);
        Quaternion toRoation = Quaternion.Euler(0, 0, 0);

        if (transform.position != toPosition)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 15f);
        }

        if (transform.rotation != toRoation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, toRoation, Time.deltaTime * 15f);
        }
    }

    void OnMouseDrag()
    {
        if (selected == false)
        {
        }

        selected = true;

        // 마우스의 현재 화면 좌표를 가져옵니다.
        Vector3 mouseScreenPosition = Input.mousePosition;

        // 카메라에서 오브젝트까지의 실제 Z 축 거리를 설정합니다.
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;

        // 화면 좌표를 월드 좌표로 변환합니다.
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Quaternion toRotation = Quaternion.Euler((mouseWorldPosition.y - transform.position.y) * 90, -(mouseWorldPosition.x - transform.position.x) * 90, 0);

        // 오브젝트를 마우스 위치로 부드럽게 이동시키되, y 위치는 고정합니다.
        mouseWorldPosition.y = transform.position.y; // y 위치 고정

        // 오브젝트를 마우스 위치로 부드럽게 이동시킵니다.
        transform.position = Vector3.Lerp(transform.position, mouseWorldPosition, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 15f);
    }
}
