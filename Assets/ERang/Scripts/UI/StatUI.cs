using UnityEngine;
using TMPro;

public class StatUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetText(string text)
    {
        this.text.text = text;
    }
}
