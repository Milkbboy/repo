using UnityEngine;

namespace ERang
{
    /// <summary>
    /// Player 씬 전환 시에도 유지되도록 설정
    /// - Editor Hierachy에 GameObject 생성 후 해당 스크립트를 추가
    /// </summary>
    public class PlayerLoader : MonoBehaviour
    {
        public GameObject PlayerPrefab;

        void Awake()
        {
            if (Player.Instance == null)
            {
                GameObject playerObject = Instantiate(PlayerPrefab);
                DontDestroyOnLoad(playerObject);

                GameLogger.Log(LogCategory.DEBUG, "Player를 씬 전환 시에도 유지되도록 설정");
            }
        }
    }
}