using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환 기능을 사용하기 위해 추가

public class NextScene : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName; // 다음 씬의 이름 (인스펙터에서 설정 가능)

    
    public void Play()
    {
        SceneManager.LoadScene(nextSceneName);
        
    }

}
