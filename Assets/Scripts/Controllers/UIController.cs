using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;

public enum AnnouncementType
{
    ScrollLR,
    DropBounce,
    StaticBreathing,
    StaticFrame
}

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Regions")]
    [SerializeField] GameObject hotBar;
    [SerializeField] GameObject info;
    [SerializeField] GameObject quitScreen;
    [SerializeField] GameObject cardsScreen;
    [SerializeField] GameObject announcementBar;
    [SerializeField] GameObject gameOverScreenHost;
    [SerializeField] GameObject gameOverScreenNonHost;
    [SerializeField] GameObject chatPanel;
    [SerializeField] GameObject actionInfoPanel;
    [Header("Info")]
    [SerializeField] GameObject infoPanelPlayerList;
    [SerializeField] GameObject playerInfoCard;
    [Header("Card")]
    [SerializeField] float cardsSpacing = 100f;
    [SerializeField] float cardsAnimationDuration = 0.05f;
    [Header("Announcement")]
    [SerializeField] TMP_Text announcementText;
    [SerializeField] float announcementPauseTime = 1f;
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
    [Header("Action Panel")]
    [SerializeField] Button ActionPanelConfirm;
    [SerializeField] Button ActionPanelClear;
    [Header("Camera Position")]
    [SerializeField] TMP_Text cameraHeightText;
    [Header("Other")]
    [SerializeField] GameObject playerCamera;
    [SerializeField] PlayerController playerController;


    #region Non Serialized Variables
    List<GameObject> gameOverPlayerListObjects = new List<GameObject>();
    List<GameObject> infoPanelPlayerListObjects = new List<GameObject>();
    List<GameObject> cards = new List<GameObject>();
    float originalAnnouncementTextSize = 36;
    private bool newAnnouncementQueued = false;
    private Queue<IEnumerator> announcementQueue = new Queue<IEnumerator>();
    private bool isPlayingAnnouncement = false;
    Player player;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = PhotonNetwork.LocalPlayer;
        InitializeUI();
    }

    void Update()
    {
        DisableCameraOnUIHover();
    }

    private void DisableCameraOnUIHover()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            bool isOverDiePanel = false;
            Vector2 mousePosition = Input.mousePosition;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePosition;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                // Check if the mouse is over a specific part of the UI (replace "YourSpecificUIPart" with the actual name or tag of your UI part)
                if (result.gameObject.CompareTag("UIDiePanel"))
                {
                    isOverDiePanel = true;
                    break;
                }
            }
            if(!isOverDiePanel)
            {
                playerCamera.GetComponent<CameraController>().ToggleCameraCanMove(false);
            } else
            {
                playerCamera.GetComponent<CameraController>().ToggleCameraCanMove(true);
            }
        } else
        {
            playerCamera.GetComponent<CameraController>().ToggleCameraCanMove(true);
        }
            
    }

    private void InitializeUI()
    {
        ToggleChat();
        ToggleHotbar(false);
        ToggleRollButton(false);
        ToggleBuildButton(false);
        ToggleMoveButton(false);
        SetTurnTime(0);
        StopCurrentAnnouncements();
        ToggleGameOverScreen(false);
        ToggleQuitScreen();
        cardsScreen.SetActive(false);
        PopulateTurnPanel();
    }

    #region Main Functions
    public void StartTurnSetUI(float turnTime)
    {
        ToggleHotbar(true);
        ToggleRollButton(true);
        ToggleBuildButton(true);
        ToggleMoveButton(true);
        SetTurnTime(turnTime);
    }
    #endregion

    #region Info Panel

    public void PopulateTurnPanel()
    {
        ClearTurnPanel();
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject infoCard = Instantiate(playerInfoCard, infoPanelPlayerList.transform);
            infoCard.GetComponent<PlayerInfoCardItem>().SetInfo(p);
            infoPanelPlayerListObjects.Add(infoCard);
        }
    }

    public void ClearTurnPanel()
    {
        foreach (GameObject infoCard in infoPanelPlayerListObjects)
        {
            Destroy(infoCard);
        }
        infoPanelPlayerListObjects.Clear();
    }

    public void SortTurnPanelBasedOnTurnOrder(List<PlayerTurnOrder> sortedOrder)
    {
        ClearTurnPanel();
        foreach (PlayerTurnOrder pto in sortedOrder)
        {
            GameObject infoCard = Instantiate(playerInfoCard, infoPanelPlayerList.transform);
            infoCard.GetComponent<PlayerInfoCardItem>().SetInfo(pto.player);
            infoPanelPlayerListObjects.Add(infoCard);
        }
    }

    public void HighlightTurn(int turnIndex)
    {
        for(int i = 0; i < infoPanelPlayerListObjects.Count; i ++)
        {
            if(i == turnIndex)
            {
                infoPanelPlayerListObjects[i].GetComponent<PlayerInfoCardItem>().currentTurnGlow.SetActive(true);
            } else
            {
                infoPanelPlayerListObjects[i].GetComponent<PlayerInfoCardItem>().currentTurnGlow.SetActive(false);
            }
            
        }
    }

    #endregion

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

    public void ToggleGameOverScreen(bool toggle)
    {
        if(toggle == false)
        {
            ClearGameOverScreenPlayerList();
        }
        if(player.IsMasterClient)
        {
            gameOverScreenHost.SetActive(toggle);
        } else
        {
            gameOverScreenNonHost.SetActive(toggle);
        }
    }

    private void ClearGameOverScreenPlayerList()
    {
        foreach (GameObject g in gameOverPlayerListObjects)
        {
            Destroy(g);
        }
        gameOverPlayerListObjects.Clear();
    }

    #endregion

    #region Announcement
    public void PlayAnnouncement(string message, AnnouncementType type)
    {
        IEnumerator announcementCoroutine = null;
        announcementBar.SetActive(true);

        switch (type)
        {
            case AnnouncementType.ScrollLR:
                announcementCoroutine = ScrollTextLR(message);
                break;
            case AnnouncementType.DropBounce:
                announcementCoroutine = FallAndBounceText(message, 2.0f); // Set your desired timeShownAfterBounce value
                break;
            case AnnouncementType.StaticBreathing:
                announcementCoroutine = BreatheText(message);
                break;
            case AnnouncementType.StaticFrame:
                announcementCoroutine = StaticFrame(new string[] { message });
                break;
            default:
                Debug.LogError("Invalid AnnouncementType");
                return;
        }

        announcementQueue.Enqueue(announcementCoroutine);

        if (!isPlayingAnnouncement)
        {
            StartCoroutine(PlayQueuedAnnouncements());
        }
    }

    public void PlayAnnouncement(string[] message, AnnouncementType type)
    {
        IEnumerator announcementCoroutine = null;
        announcementBar.SetActive(true);

        switch (type)
        {
            case AnnouncementType.ScrollLR:
                announcementCoroutine = ScrollTextLR(message[0]);
                break;
            case AnnouncementType.DropBounce:
                announcementCoroutine = FallAndBounceText(message[0], 2.0f); // Set your desired timeShownAfterBounce value
                break;
            case AnnouncementType.StaticBreathing:
                announcementCoroutine = BreatheText(message[0]);
                break;
            case AnnouncementType.StaticFrame:
                announcementCoroutine = StaticFrame(message);
                break;
            default:
                Debug.LogError("Invalid AnnouncementType");
                return;
        }

        announcementQueue.Enqueue(announcementCoroutine);

        if (!isPlayingAnnouncement)
        {
            StartCoroutine(PlayQueuedAnnouncements());
        }
    }

    private IEnumerator PlayQueuedAnnouncements()
    {
        isPlayingAnnouncement = true;

        while (announcementQueue.Count > 0)
        {
            newAnnouncementQueued = false;
            IEnumerator currentAnnouncement = announcementQueue.Dequeue();
            yield return StartCoroutine(currentAnnouncement);
        }

        isPlayingAnnouncement = false;

        // Hide announcement bar when all announcements finish playing
        announcementBar.SetActive(false);
    }

    public void StopCurrentAnnouncements()
    {
        StopAllCoroutines();
        newAnnouncementQueued = false;
        isPlayingAnnouncement = false;
        announcementQueue.Clear();

        // Hide announcement bar when stopping announcements
        announcementBar.SetActive(false);
    }

    IEnumerator StaticFrame(string[] frames, float speed = 0.5f)
    {
        announcementText.rectTransform.anchoredPosition = new Vector2(0, 0);
        int counter = 0;
        announcementText.text = frames[counter];
        bool run = true;
        while (run)
        {
            if (newAnnouncementQueued)
            {
                run = false;
            }
            announcementText.text = frames[counter];
            if(counter + 1 > frames.Length - 1)
            {
                counter = 0;
            } else
            {
                counter++;
            }
            yield return new WaitForSeconds(speed);
        }
        yield return null;
    }

    IEnumerator ScrollTextLR(string message, float targetPositionX = 900, float speed = 800)
    {
        // Set text to left side
        announcementText.rectTransform.anchoredPosition = new Vector2(-targetPositionX, 0);
        announcementText.text = message;

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
        yield return null;
    }

    IEnumerator FallAndBounceText(string message, float timeShownAfterBounce, float targetPositionY = 3000, float speed = 6000, float bounciness = 0.05f)
    {
        announcementText.text = message;
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
        yield return null;
    }

    IEnumerator BreatheText(string message)
    {
        announcementText.text = message;
        announcementText.rectTransform.anchoredPosition = new Vector2(0, 0);
        bool run = true;
        while (run)
        {
            if (newAnnouncementQueued)
            {
                run = false;
            }

            // Grow the text
            yield return ScaleTextAnimation(originalAnnouncementTextSize, originalAnnouncementTextSize * 1.2f, 0.5f);

            // Shrink the text
            yield return ScaleTextAnimation(originalAnnouncementTextSize * 1.2f, originalAnnouncementTextSize, 0.5f);
        }
        yield return null;
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

    public void ToggleHotbar(bool toggle)
    {
        hotBar.SetActive(toggle);
        actionInfoPanel.SetActive(false);
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

    public void ToggleActionPanel(bool toggle)
    {
        actionInfoPanel.SetActive(toggle);
    }

    public void ToggleMoveButton(bool toggle)
    {
        moveButton.interactable = toggle;
    }

    public void ToggleActionPanelConfirm(bool toggle)
    {
        ActionPanelConfirm.interactable = toggle;
    }

    public void OnBuildButtonClick()
    {
        actionInfoPanel.GetComponentInChildren<TMP_Text>().enabled = true;
        ToggleActionPanel(true);
        actionInfoPanel.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(30, 50, 0);
        Board.Instance.selectionMode = SelectionMode.Build;
    }

    public void OnMoveButtonClick()
    {
        actionInfoPanel.GetComponentInChildren<TMP_Text>().enabled = false;
        ToggleActionPanel(true);
        actionInfoPanel.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-20, 50, 0);
        Board.Instance.selectionMode = SelectionMode.Move;
    }

    public void OnRollButtonClick()
    {
        PlayerController.Instance.RollForActionDie();
        ToggleRollButton(false);
    }

    public void OnActionPanelConfirm()
    {
        Board.Instance.ConfirmAction();
        ToggleActionPanel(false);
    }

    public void OnActionPanelClear()
    {
        Board.Instance.ClearAction();
    }

    public void SetBlocksLeftToBuild(int blocksToBuild)
    {
        actionInfoPanel.GetComponentInChildren<TMP_Text>().text = "x" + blocksToBuild;
    }
    

    #endregion

    #region Quit
    public void ToggleQuitScreen()
    {
        quitScreen.SetActive(!quitScreen.activeSelf);
    }
    #endregion

    #region Cards
    public void AddCard()
    {
        Array values = Enum.GetValues(typeof(CardType));
        CardType randomCardType = (CardType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        GameObject cardObj = Instantiate(GameAssets.i.card_, Vector3.zero, Quaternion.identity);
        cardObj.transform.SetParent(cardsScreen.transform, false);
        cardObj.GetComponent<Card>().SetCardType(randomCardType);
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

    #region Camera
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
    #endregion
}
