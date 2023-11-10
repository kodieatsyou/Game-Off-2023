using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
	[SerializeField] TMP_Text text;

	public RoomInfo info;

	public void SetInfo(RoomInfo _info)
	{
		info = _info;
		text.text = string.Format("{0}    {1}/{2}", _info.Name, _info.PlayerCount, _info.MaxPlayers);
	}

	public void OnClick()
	{
		Lobby.Instance.JoinRoom(info);
	}
}
