using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;

public class UIController : MonoBehaviour
{

    public GameObject hotBar;
    public GameObject announcement;
    public GameObject info;
    public GameObject menuScreen;
    public GameObject rulesScreen;

    [SerializeField] TMP_Text playerName;

    Player player;


    // Start is called before the first frame update
    void Start()
    {
        hotBar.SetActive(false);
        player = PhotonNetwork.LocalPlayer;
        if (player == PhotonNetwork.LocalPlayer)
        {
            // Just testing setting names
            playerName.text = player.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayUIAnnouncement(string text)
    {
        announcement.GetComponentInChildren<TMP_Text>().text = text;
        announcement.GetComponent<Animator>().SetTrigger("PlayAnnouncement");
    }

    public void SetTurnTime()
    {
        int seconds = 65;

        int minutes = TimeSpan.FromSeconds(seconds).Minutes;
        seconds = TimeSpan.FromSeconds(seconds).Seconds;

        string minutesString = "";
        if(minutes < 10)
        {
            minutesString = "0" + minutes;
        } else
        {
            minutesString = "" + minutes;
        }
        string secondsString = "";
        if(seconds < 10)
        {
            secondsString = "0" + seconds;
        } else
        {
            secondsString = "" + seconds;
        }
        hotBar.transform.GetChild(4).GetComponent<TMP_Text>().text = minutesString + ":" + secondsString;
    }

    public void ToggleHotbar()
    {
        hotBar.SetActive(!hotBar.activeSelf);
    }

    public void ToggleMenuScreen()
    {
        menuScreen.SetActive(!menuScreen.activeSelf);
    }

    public void ToggleRulesScreen()
    {
        rulesScreen.SetActive(!rulesScreen.activeSelf);
    }
}
