using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneOnAnyKey : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // ���� ���� �̸� (�ν����Ϳ��� ���� ����)

    [SerializeField]
    private float delayInSeconds = 3.0f; // �� ��ȯ ������ �ð� (��) (�ν����Ϳ��� ���� ����)

    void Update()
    {
        // �ƹ� Ű�� �Է¹����� ���� ���� �ε�
        if (Input.anyKeyDown)
        {
            StartCoroutine(LoadNextSceneWithDelay());
        }
    }

    IEnumerator LoadNextSceneWithDelay()
    {
        // ������ �ð���ŭ ���
        yield return new WaitForSeconds(delayInSeconds);

        // ���� �� �ε�
        SceneManager.LoadScene(nextSceneName);
    }
}
