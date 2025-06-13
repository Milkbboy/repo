using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneOnAnyKey : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // 다음 씬 이름 (네비게이션 씬 이름)

    [SerializeField]
    private float delayInSeconds = 3.0f; // 로고 표시 시간 후 다음 씬으로 이동 (초) (네비게이션 씬 이름)

    void Update()
    {
        // 키 입력 시 다음 씬으로 이동
        if (Input.anyKeyDown)
        {
            StartCoroutine(LoadNextSceneWithDelay());
        }
    }

    IEnumerator LoadNextSceneWithDelay()
    {
        // 로고 표시 시간 후 다음 씬으로 이동
        yield return new WaitForSeconds(delayInSeconds);

        // 다음 씬으로 이동
        SceneManager.LoadScene(nextSceneName);
    }
}
