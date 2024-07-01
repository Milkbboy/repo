using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int repositoryPosition;
    public bool selected;

    public int handPosition, deckPosition, fieldPosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if (!selected)
        // {
        //     MoveToPosition();
        // }

        if (Input.GetMouseButtonUp(0))
        {
            selected = false;
        }
    }

    void OnMouseDrag()
    {
        selected = true;

        // 마우스의 현재 화면 좌표를 가져옵니다.
        Vector3 mouseScreenPosition = Input.mousePosition;

        // 카메라에서 오브젝트까지의 실제 Z 축 거리를 설정합니다.
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;

        // 화면 좌표를 월드 좌표로 변환합니다.
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // 오브젝트를 마우스 위치로 부드럽게 이동시키되, y 위치는 고정합니다.
        mouseWorldPosition.y = transform.position.y; // y 위치 고정

        // 오브젝트를 마우스 위치로 부드럽게 이동시킵니다.
        transform.position = Vector3.Lerp(transform.position, mouseWorldPosition, Time.deltaTime * 15f);
    }
}
