using System.Collections;

namespace ERang
{
    public interface IBattleController
    {
        // 배틀 상태
        Player Player { get; }
        int TurnCount { get; }

        // 배틀 흐름 제어
        void InitializeBattle();
        IEnumerator StartBattle();
        IEnumerator EndBattle(bool isWin);

        // UI 업데이트
        void UpdateSatietyGauge(int amount);
    }
}