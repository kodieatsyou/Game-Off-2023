using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;
    Player player;

    public void SetInfo(Player _player)
    {
        player = _player;
        text.text = _player.NickName;

        if (player.IsMasterClient)
        {
            text.text = text.text + " (Host)";
        }

        if (PhotonNetwork.NickName == _player.NickName)
        {
            text.color = Color.red;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
	{
        if (player.IsMasterClient)
        {
            text.text = text.text + " (Host)";
        }
	}
}
