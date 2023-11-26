using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
    Barrier,
    Levitate,
    Ninja,
    Punch,
    Steal,
    Switch,
    Taunt,
    TimeStop
}

public class Card : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;
    public Image art;

    public void SetCardType(CardType type)
    {
        title.text = type.ToString();
        description.text = GameAssets.i.card_descriptions_[type];
        art.sprite = GameAssets.i.card_art_[type];
    }
}
