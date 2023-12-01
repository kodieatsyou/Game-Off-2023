using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameOverPlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text heightText;
    [SerializeField] GameObject winnerGlow;

    [SerializeField] float speed = 1f;
    [SerializeField] float startScale = 0.5f;
    [SerializeField] float scaleIncrementFactor = 0.1f;
    [SerializeField] float maxScale = 1f;

    //[SerializeField] AudioClip incrementSound;
    //[SerializeField] float pitchIncrement = 0.1f;
    //[SerializeField] AudioSource audioSource;

    private int height;
    private bool winner;
    public void SetInfo(string testName, int heightClimbed, bool isWinner)
    {
        nameText.text = testName;
        winner = isWinner;
        height = heightClimbed;
    }

    public void SetHeightText(int height, float textScale) {
        if(height <= this.height) {
            if(height == this.height && winner) {
                HighlightAsWinner();
            }
            heightText.text = height.ToString();
            heightText.transform.localScale = new Vector3(textScale, textScale, 1f);
        }
    }

    public void HighlightAsWinner() {
        winnerGlow.SetActive(true);
    }
}
