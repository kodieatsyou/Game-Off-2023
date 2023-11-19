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
    [Header("Essential Objects")]
    public GameObject hotBar;
    public GameObject announcement;
    public GameObject info;
    public GameObject menuScreen;
    public GameObject cardsScreen;
    public GameObject dieViewer;
    public GameObject diePit;
    [Header("Cards")]
    public List<GameObject> cards;
    public float cardsSpacing = 100f;
    public float cardsAnimationDuration = 0.05f;
    public GameObject testCard;
    [Header("Dice")]
    public float rollForce = 5f;


    [SerializeField] TMP_Text playerName;

    Player player;


    // Start is called before the first frame update
    void Start()
    {
        //hotBar.SetActive(false);
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

    public void AddCard(GameObject card)
    {
        GameObject cardObj = Instantiate(card, Vector3.zero, Quaternion.identity);
        cardObj.transform.SetParent(cardsScreen.transform, false);
        cards.Add(cardObj);
    }

    public void TestAddCard()
    {
        GameObject cardObj = Instantiate(testCard, Vector3.zero, Quaternion.identity);
        cardObj.transform.SetParent(cardsScreen.transform, false);
        cards.Add(cardObj);
    }

    public void ToggleCardsScreen()
    {
        if(!cardsScreen.activeSelf)
        {
            StartCoroutine(SpreadCardsOut());
        } else
        {
            StartCoroutine(PutCardsAway());
        }
    }

    IEnumerator SpreadCardsOut()
    {
        cardsScreen.SetActive(true);
        float startX = 0;
        RectTransform parentRect = cardsScreen.GetComponent<RectTransform>();

        if (cards.Count % 2 == 0)
        {
            startX = ((cards.Count - 1) * -cardsSpacing) / 2.0f;
        }
        else
        {
            startX = Mathf.Floor(cards.Count / 2) * -cardsSpacing;
        }


        float timer = 0f;
        Vector2 stackPosition = parentRect.anchoredPosition;

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform cardRect = cards[i].GetComponent<RectTransform>();
            cardRect.anchoredPosition = stackPosition;
        }

        while (timer < cardsAnimationDuration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < cards.Count; i++)
            {
                RectTransform cardRect = cards[i].GetComponent<RectTransform>();
                float t = timer / cardsAnimationDuration;
                cardRect.anchoredPosition = Vector2.Lerp(stackPosition, new Vector2(startX + (i * cardsSpacing), 0f), t);
            }

            yield return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform cardRect = cards[i].GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(startX + (i * cardsSpacing), 0f);
        }
    }


    IEnumerator PutCardsAway()
    {
        float timer = 0f;
        RectTransform parentRect = cardsScreen.GetComponent<RectTransform>();
        Vector2 stackPosition = parentRect.anchoredPosition;

        while (timer < cardsAnimationDuration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < cards.Count; i++)
            {
                RectTransform cardRect = cards[i].GetComponent<RectTransform>();
                float t = timer / cardsAnimationDuration;
                cardRect.anchoredPosition = Vector2.Lerp(new Vector2(cards[i].GetComponent<RectTransform>().anchoredPosition.x, 0f), stackPosition, t);
            }

            yield return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform cardRect = cards[i].GetComponent<RectTransform>();
            cardRect.anchoredPosition = stackPosition;
        }
        cardsScreen.SetActive(false);
    }
}
