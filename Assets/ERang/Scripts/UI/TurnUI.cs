using UnityEngine;
using TMPro;

public class TurnUI : MonoBehaviour
{
    public TextMeshProUGUI turnCounts;
    public TextMeshProUGUI gold;

    public void SetTurn(int count)
    {
        turnCounts.text = count.ToString();
    }

    public void SetGold(int gold)
    {
        this.gold.text = gold.ToString();
    }
}
