using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneOnAnyKey : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // 다음 씬의 이름 (인스펙터에서 설정 가능)

    [SerializeField]
    private float delayInSeconds = 3.0f; // 씬 전환 딜레이 시간 (초) (인스펙터에서 설정 가능)

    void Update()
    {
        // 아무 키나 입력받으면 다음 씬을 로드
        if (Input.anyKeyDown)
        {
            StartCoroutine(LoadNextSceneWithDelay());
        }
    }

    IEnumerator LoadNextSceneWithDelay()
    {
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(delayInSeconds);

        // 다음 씬 로드
        SceneManager.LoadScene(nextSceneName);
    }
}
