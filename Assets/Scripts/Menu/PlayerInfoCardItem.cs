using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using Photon.Pun;

public class PlayerInfoCardItem : MonoBehaviour
{
    Player p;
    public TMP_Text playerName;
    public GameObject currentTurnGlow;
    public void SetInfo(Player p)
    {
        this.p = p;
        transform.name = p.NickName;
        playerName.text = p.NickName;
        currentTurnGlow.SetActive(false);
    }
}
