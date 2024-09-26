using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // �� ��ȯ ����� ����ϱ� ���� �߰�

public class NextScene : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // ���� ���� �̸� (�ν����Ϳ��� ���� ����)

    public void Play()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void Play(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
