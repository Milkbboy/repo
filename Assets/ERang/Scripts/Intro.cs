using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [SerializeField]
    public float logoDisplayTime = 3.0f; // 로고 표시 시간 (초)

    [SerializeField]
    public string nextSceneName; // 다음 씬의 이름 (인스펙터에서 설정 가능)

    private bool isKeyPressed = false; // 키 입력이 감지되었는지 여부

    void Start()
    {
        // 로고 표시 시간이 지난 후 타이틀 씬으로 전환
        StartCoroutine(LoadNextSceneAfterDelay(logoDisplayTime));
    }

    void Update()
    {
        // 아무 키나 입력받으면 바로 다음 씬으로 전환
        if (Input.anyKeyDown)
        {
            isKeyPressed = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(delay);

        // 키 입력이 없었을 경우에만 씬을 전환
        if (!isKeyPressed)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

}

