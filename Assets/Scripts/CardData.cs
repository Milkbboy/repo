using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "MyGame/CardData", order = 1)]
public class CardData : ScriptableObject
{
    public string uid;

    [Header("Card Info")]
    public string cardName;
    public string description;

    [Header("Card Stats")]
    public int attack;
    public int hp;
}
