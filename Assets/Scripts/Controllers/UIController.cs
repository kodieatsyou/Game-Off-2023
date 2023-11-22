using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;

public enum AnnouncementType
{
    ScrollLR,
    DropBounce,
    StaticBreathing
}

public class UIController : MonoBehaviour
{
    [Header("Regions")]
    [SerializeField] GameObject hotBar;
    [SerializeField] GameObject info;
    [SerializeField] GameObject menuScreen;
    [SerializeField] GameObject cardsScreen;
    [SerializeField] GameObject announcementBar;
    [SerializeField] GameObject gameOverScreenHost;
    [SerializeField] GameObject gameOverScreenNonHost;
    [SerializeField] GameObject chatPanel;
    [SerializeField] GameObject actionInfoPanel;
    [Header("Info")]
    [SerializeField] TMP_Text playerName;
    [Header("Card")]
    [SerializeField] float cardsSpacing = 100f;
    [SerializeField] float cardsAnimationDuration = 0.05f;
    [Header("Announcement")]
    [SerializeField] TMP_Text announcementText;
    [SerializeField] float announcementSpeed = 5f;
    [SerializeField] float announcementPauseTime = 3f;
    [SerializeField] int announcementPos = 900;
    [Header("Game Over Screen")]
    [SerializeField] GameObject gameOverPlayerListObject;
    [Header("Chat")]
    [SerializeField] GameObject chatMessageObject;
    [SerializeField] GameObject chatContent;
    [SerializeField] GameObject chatTypeBox;
    [SerializeField] GameObject unreadChatIco;
    [Header("Hotbar")]
    [SerializeField] Button rollButton;
    [SerializeField] Button buildButton;
    [SerializeField] Button moveButton;
    [SerializeField] Button cardsButton;
    [Header("Camera Position")]
    [SerializeField] TMP_Text cameraHeightText;
    [Header("Other")]
    [SerializeField] GameObject playerCamera;

    #region Non Serialized Variables
    List<GameObject> gameOverPlayerListObjects = new List<GameObject>();
    List<GameObject> cards = new List<GameObject>();
    Coroutine currentAnnouncement;
    float originalAnnouncementTextSize;
    GameObject currentBlockCursor;
    Player player;
    #endregion

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

        ToggleChat();
    }

    #region Chat
    public void ToggleChat()
    {
        if (unreadChatIco.activeSelf)
        {
            unreadChatIco.SetActive(false);
        }
        chatPanel.SetActive(!chatPanel.activeSelf);
    }

    #endregion

    #region Game Over Screen
    public void AddPlayertoGameOverBoard(string name, int heightClimbed, bool isWinner)
    {
        GameObject listContent = null;
        if (player.IsMasterClient)
        {
            listContent = gameOverScreenHost.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        } else
        {
            listContent = gameOverScreenNonHost.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        }
        GameObject playerListItem = Instantiate(gameOverPlayerListObject, listContent.transform);
        playerListItem.GetComponent<GameOverPlayerListItem>().SetInfo(name, heightClimbed, isWinner);
        gameOverPlayerListObjects.Add(playerListItem);
    }

    public void ShowGameOverScreen()
    {
        /*if(player.IsMasterClient)
        {
            gameOverScreenHost.SetActive(true);
        } else
        {
            gameOverScreenNonHost.SetActive(true);
        }*/

        gameOverScreenHost.SetActive(true);

    }

    public void HideGameOverScreen()
    {
        if (player.IsMasterClient)
        {
            gameOverScreenHost.SetActive(false);
        } else
        {
            gameOverScreenNonHost.SetActive(false);
        }

        foreach(GameObject g in gameOverPlayerListObjects)
        {
            Destroy(g);
        }
        gameOverPlayerListObjects.Clear();
    }

    #endregion

    #region Announcement
    public void PlayAnnouncement(string message, AnnouncementType type)
    {
        if (currentAnnouncement != null)
        {
            StopCoroutine(currentAnnouncement);
        }

        announcementBar.SetActive(true);

        announcementText.text = message;

        switch(type)
        {
            case AnnouncementType.ScrollLR:
                currentAnnouncement = StartCoroutine(ScrollTextLR());
                break;
            case AnnouncementType.DropBounce:
                currentAnnouncement = StartCoroutine(FallAndBounceText(1.0f));
                break;
            case AnnouncementType.StaticBreathing:
                currentAnnouncement = StartCoroutine(BreatheText());
                break;
        }
    }

    public void StopAnnouncement()
    {
        if (currentAnnouncement != null)
        {
            StopCoroutine(currentAnnouncement);
            currentAnnouncement = null;
        }
        announcementText.GetComponent<TextMeshProUGUI>().fontSize = originalAnnouncementTextSize;
        announcementText.rectTransform.anchoredPosition = new Vector2(0, 0);
        announcementBar.SetActive(false);
    }

    IEnumerator ScrollTextLR(float targetPositionX = 900, float speed = 500)
    {
        // Set text to left side
        announcementText.rectTransform.anchoredPosition = new Vector2(-targetPositionX, 0);

        // Go to the middle
        while (Mathf.Abs(announcementText.rectTransform.anchoredPosition.x) > 0.1f)
        {
            float step = speed * Time.deltaTime;
            float newX = Mathf.MoveTowards(announcementText.rectTransform.anchoredPosition.x, 0, step);
            announcementText.rectTransform.anchoredPosition = new Vector2(newX, 0);
            yield return null;
        }

        // Set text to the middle position
        announcementText.rectTransform.anchoredPosition = new Vector2(0, 0);

        // Wait for the pause time
        yield return new WaitForSeconds(announcementPauseTime);

        // Scroll to the positive target position
        while (announcementText.rectTransform.anchoredPosition.x < targetPositionX - 0.1f)
        {
            float step = speed * Time.deltaTime;
            float newX = Mathf.MoveTowards(announcementText.rectTransform.anchoredPosition.x, targetPositionX, step);

            // Ensure that the movement does not exceed the target position
            if (newX > targetPositionX)
            {
                newX = targetPositionX;
            }

            announcementText.rectTransform.anchoredPosition = new Vector2(newX, 0);
            yield return null;
        }

        // Ensure that the final position is exactly at the target position
        announcementText.rectTransform.anchoredPosition = new Vector2(targetPositionX, 0);
        StopAnnouncement();
    }

    IEnumerator FallAndBounceText(float timeShownAfterBounce, float targetPositionY = 3000, float speed = 6000, float bounciness = 0.05f)
    {
        announcementText.rectTransform.anchoredPosition = new Vector2(0, targetPositionY);

        //Fall to middle
        while (Mathf.Abs(announcementText.rectTransform.anchoredPosition.y) > 0.1f)
        {
            float step = speed * Time.deltaTime;
            float newY = Mathf.MoveTowards(announcementText.rectTransform.anchoredPosition.y, 0, step);
            announcementText.rectTransform.anchoredPosition = new Vector2(0, newY);
            yield return null;
        }

        announcementText.rectTransform.anchoredPosition = new Vector2(0, 0);

        // Bounce
        float bounceHeight = targetPositionY * bounciness;
        float timeToBounce = 0.5f;
        float elapsedTime = 0.0f;

        while (elapsedTime < timeToBounce)
        {
            float bounceY = Mathf.Sin(elapsedTime / timeToBounce * Mathf.PI) * bounceHeight;
            announcementText.rectTransform.anchoredPosition = new Vector2(0, bounceY);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        announcementText.rectTransform.anchoredPosition = new Vector2(0, 0);
        yield return new WaitForSeconds(timeShownAfterBounce);
        StopAnnouncement();
    }

    IEnumerator BreatheText()
    {
        originalAnnouncementTextSize = announcementText.GetComponent<TextMeshProUGUI>().fontSize;
        while (true)
        {
            // Grow the text
            yield return ScaleTextAnimation(originalAnnouncementTextSize, originalAnnouncementTextSize * 1.2f, 0.5f);

            // Shrink the text
            yield return ScaleTextAnimation(originalAnnouncementTextSize * 1.2f, originalAnnouncementTextSize, 0.5f);
        }
    }

    IEnumerator ScaleTextAnimation(float startScale, float endScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            announcementText.GetComponent<TextMeshProUGUI>().fontSize = Mathf.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        announcementText.GetComponent<TextMeshProUGUI>().fontSize = endScale;
    }

    #endregion

    #region Hotbar

    public void SetBlockCursor(GameObject cursor)
    {
        currentBlockCursor = cursor;
    }

    public GameObject GetBlockCursor()
    {
        return currentBlockCursor;
    }

    public void SetTurnTime(float seconds)
    {

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

    public void ToggleRollButton(bool toggle)
    {
        rollButton.interactable = toggle;
    }

    public void ToggleBuildButton(bool toggle)
    {
        buildButton.interactable = toggle;
    }

    public void BuildButtonOnClick()
    {
        if(currentBlockCursor == GameAssets.i.build_cursor_)
        {
            currentBlockCursor = null;
            actionInfoPanel.SetActive(false);
            BoardManagerNew.Instance.ToggleBuildableBlocksIsSelectable(false);
        } else
        {
            actionInfoPanel.SetActive(true);
            SetBlockCursor(GameAssets.i.build_cursor_);
            BoardManagerNew.Instance.ToggleBuildableBlocksIsSelectable(true);
            actionInfoPanel.GetComponentInChildren<TMP_Text>().enabled = true;
        }
    }
    public void SetBlocksLeft(int blocks)
    {
        actionInfoPanel.GetComponentInChildren<TMP_Text>().text = "X" + blocks;
    }

    public void MoveButtonOnClick()
    {
        if (currentBlockCursor == GameAssets.i.move_cursor_)
        {
            currentBlockCursor = null;
            actionInfoPanel.SetActive(false);
            BoardManagerNew.Instance.ToggleBuildableBlocksIsSelectable(false);
        }
        else
        {
            actionInfoPanel.SetActive(true);
            SetBlockCursor(GameAssets.i.move_cursor_);
            BoardManagerNew.Instance.ToggleBuildableBlocksIsSelectable(true);
            actionInfoPanel.GetComponentInChildren<TMP_Text>().enabled = false;
        }
    }

    public void ToggleMoveButton(bool toggle)
    {
        moveButton.interactable = toggle;
    }

    public void ToggleHotbar()
    {
        hotBar.SetActive(!hotBar.activeSelf);
    }

    public void ClearSelectedBlocks()
    {
        BoardManagerNew.Instance.ClearSelectedBlocks();
    }
    #endregion

    #region Menu
    public void ToggleMenuScreen()
    {
        menuScreen.SetActive(!menuScreen.activeSelf);
    }
    #endregion

    #region Cards
    public void AddCard(GameObject card)
    {
        GameObject cardObj = Instantiate(card, Vector3.zero, Quaternion.identity);
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
    #endregion


    public void MoveCameraUp()
    {
        int level = playerCamera.GetComponent<CameraController>().MoveCameraUpOneBoardLevel();
        cameraHeightText.text = level.ToString();
    }

    public void MoveCameraDown()
    {
        int level = playerCamera.GetComponent<CameraController>().MoveCameraDownOneBoardLevel();
        cameraHeightText.text = level.ToString();
    }
}
