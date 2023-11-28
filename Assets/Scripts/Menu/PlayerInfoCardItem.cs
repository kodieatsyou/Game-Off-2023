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
    [SerializeField] TMP_Text playerName;
    [SerializeField] GameObject currentTurnGlow;
    public void SetInfo(Player p)
    {
        this.p = p;
        transform.name = p.NickName;
        playerName.text = p.NickName;
        currentTurnGlow.SetActive(false);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties["CurrentPlayerTurn"] != null)
        {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentPlayerTurn"] == p.ActorNumber)
            {
                currentTurnGlow.SetActive(true);
            }
        }
        
    }
}
