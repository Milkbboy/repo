using UnityEngine;
using TMPro;

public class TurnUI : MonoBehaviour
{
    public TextMeshProUGUI turnCounts;
    public TextMeshProUGUI gold;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetTurn(int count)
    {
        turnCounts.text = count.ToString();
    }

    public void SetGold(int gold)
    {
        this.gold.text = gold.ToString();
    }
}
