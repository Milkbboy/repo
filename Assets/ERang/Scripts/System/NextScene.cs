using UnityEngine;
using UnityEngine.SceneManagement; // 다음 씬으로 이동하기 위해 추가

public class NextScene : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // 다음 씬 이름 (네비게이션 씬 이름)

    public void Play()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void Play(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
