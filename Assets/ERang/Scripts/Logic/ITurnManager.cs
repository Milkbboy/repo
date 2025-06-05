using System.Collections;

namespace ERang
{
    public interface ITurnManager
    {
        int TurnCount { get; }
        bool IsTurnEndProcessing { get; }

        float TurnStartActionDelay { get; set; }
        float BoardTurnEndDelay { get; set; }

        IEnumerator StartTurn();
        IEnumerator EndTurn();
    }
}