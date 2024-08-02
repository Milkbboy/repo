using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckUI : MonoBehaviour
{
    public TextMeshProUGUI cardCounts;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCount(int count)
    {
        cardCounts.text = count.ToString();
    }
}
