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
    [SerializeField] GameObject host;
    Player player;

    public void SetInfo(Player _player)
    {
        player = _player;
        text.text = _player.NickName;
        host.SetActive(player.IsMasterClient);

        if (PhotonNetwork.NickName == _player.NickName)
        {
            text.color = Color.white;
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
        host.SetActive(player.IsMasterClient);
	}
}
