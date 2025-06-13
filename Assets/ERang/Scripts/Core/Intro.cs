using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [SerializeField]
    public float logoDisplayTime = 3.0f; // 로고 표시 시간 (초)

    [SerializeField]
    public string nextSceneName; // 다음 씬 이름 (네비게이션 씬 이름)

    private bool isKeyPressed = false; // 키 입력 여부

    void Start()
    {
        // 로고 표시 시간 후 다음 씬으로 이동
        StartCoroutine(LoadNextSceneAfterDelay(logoDisplayTime));
    }

    void Update()
    {
        // 키 입력 시 다음 씬으로 이동
        if (Input.anyKeyDown)
        {
            isKeyPressed = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        // 로고 표시 시간 후 다음 씬으로 이동
        yield return new WaitForSeconds(delay);

        // 키 입력 시 다음 씬으로 이동
        if (!isKeyPressed)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

}

