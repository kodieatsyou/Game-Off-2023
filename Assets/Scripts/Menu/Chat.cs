using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chat : MonoBehaviourPun
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject chatMessageObject;
    [SerializeField] private GameObject chatTypeBox;
    [SerializeField] private GameObject unreadChatIco;

    [PunRPC]
    public void PostChatMessage(Player player, string message)
    {
        if(!chatPanel.activeSelf)
        {
            if(!unreadChatIco.activeSelf)
            {
                unreadChatIco.SetActive(true);
            }
        }
        GameObject chatMessage = Instantiate(chatMessageObject, transform);
        TextMeshProUGUI textMeshPro = chatMessage.GetComponent<TextMeshProUGUI>();

        string fullMessage = string.Format("<color=#FF0000> [{0} : {1}] </color> \n {2}", DateTime.Now.ToString("h:mm tt"), player.NickName, message);
        textMeshPro.text = fullMessage;

        // Force an immediate update of all canvases
        Canvas.ForceUpdateCanvases();

        // Get the preferred height of the text
        float preferredHeight = textMeshPro.preferredHeight;

        // Adjust the RectTransform height to fit the content
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, preferredHeight);
    }

    public void SendChatMessage()
    {
        photonView.RPC("PostChatMessage", RpcTarget.All, PhotonNetwork.LocalPlayer, chatTypeBox.GetComponent<TMP_InputField>().text);
        chatTypeBox.GetComponent<TMP_InputField>().text = "";
    }
}
