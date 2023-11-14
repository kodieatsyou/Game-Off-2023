using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
	[SerializeField] TMP_Text roomName;
	[SerializeField] TMP_Text playerCount;

	public RoomInfo info;

	public void SetInfo(RoomInfo _info)
	{
		info = _info;
		roomName.text = _info.Name;
		playerCount.text = string.Format("{0}/{1}", _info.PlayerCount, _info.MaxPlayers);
	}

	public void OnClick()
	{
		Lobby.Instance.JoinRoom(info);
	}
}
