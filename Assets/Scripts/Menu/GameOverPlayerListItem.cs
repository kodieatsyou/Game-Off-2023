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

    private void Start()
    {
        StartCoroutine(DisplayHeight(height, winner));
    }
    public void SetInfo(string testName, int heightClimbed, bool isWinner)
    {
        nameText.text = testName;
        winner = isWinner;
        height = heightClimbed;
    }

    IEnumerator DisplayHeight(int targetHeight, bool isWinner)
    {
        int currentHeight = 0;
        float currentScale = startScale;

        while (currentHeight < targetHeight)
        {
            currentHeight++;
            heightText.text = currentHeight.ToString();

            heightText.transform.localScale = new Vector3(currentScale, currentScale, 1f);

            /*if (incrementSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(incrementSound);
                audioSource.pitch += pitchIncrement;
            }*/

            currentScale = currentScale + scaleIncrementFactor;
            currentScale = Mathf.Clamp(currentScale, startScale, maxScale);
            yield return new WaitForSeconds(1f / speed);
            speed += 0.5f;
        }

        if(isWinner)
        {
            winnerGlow.SetActive(true);
        }
    }
}
