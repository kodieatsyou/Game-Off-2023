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
    Taunt,
    TimeStop
}

/*

Steal,
    Switch,

    */

public class Card : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;
    public Image art;

    CardType type;

    public void SetCardType(CardType type)
    {
        this.type = type;
        title.text = type.ToString();
        description.text = GameAssets.i.card_descriptions_[type];
        art.sprite = GameAssets.i.card_art_[type];
    }

    public void UseCard() {
        PlayerController.Instance.UseCard(type);
    }

    
}
