using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [SerializeField]
    public float logoDisplayTime = 3.0f; // �ΰ� ǥ�� �ð� (��)

    [SerializeField]
    public string nextSceneName; // ���� ���� �̸� (�ν����Ϳ��� ���� ����)

    private bool isKeyPressed = false; // Ű �Է��� �����Ǿ����� ����

    void Start()
    {
        // �ΰ� ǥ�� �ð��� ���� �� Ÿ��Ʋ ������ ��ȯ
        StartCoroutine(LoadNextSceneAfterDelay(logoDisplayTime));
    }

    void Update()
    {
        // �ƹ� Ű�� �Է¹����� �ٷ� ���� ������ ��ȯ
        if (Input.anyKeyDown)
        {
            isKeyPressed = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        // ������ �ð���ŭ ���
        yield return new WaitForSeconds(delay);

        // Ű �Է��� ������ ��쿡�� ���� ��ȯ
        if (!isKeyPressed)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

}

