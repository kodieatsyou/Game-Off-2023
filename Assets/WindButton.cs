using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public enum WindDir
{
    North,
    East,
    South,
    West
}

public class WindButton : MonoBehaviour
{
    public WindDir dir;
    public Color unhoveredColor;
    public Color hoveredColor;

    private bool isHovered;
    public void Initialize(WindDir dir)
    {
        this.dir = dir;
        SetPositionAndRotation();
        gameObject.SetActive(false);
    }

    void SetPositionAndRotation()
    {
        double offset1 = (Board.Instance.baseSize + (2.5f / 2.0f));
        double offset2 = ((Board.Instance.baseSize * 2.5f) + offset1);
        float yPos = Board.Instance.GetBoardMiddlePosAtYLevel(Board.Instance.heightSize / 2).y;

        switch (dir)
        {
            case WindDir.North:
                gameObject.transform.position = new Vector3((float)-offset1, yPos, (float)offset1);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case WindDir.East:
                gameObject.transform.position = new Vector3((float)offset1, yPos, (float)offset2);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case WindDir.South:
                gameObject.transform.position = new Vector3((float)offset2, yPos, (float)offset1);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case WindDir.West:
                gameObject.transform.position = new Vector3((float)offset1, yPos, -(float)offset1);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
        }
    }
    void Update()
    {
        if(isHovered) {
            if(Input.GetMouseButtonDown(0)) {
                BoardManager.Instance.BMPhotonView.RPC("RPCBoardManagerDoWind", RpcTarget.All, dir);
                UIController.Instance.ToggleWindDirectionButtons(false);
                UIController.Instance.ToggleWindButton(false);
                PlayerController.Instance.ActionsRemaining -= 1;
            }
        }
    }

    private void OnMouseEnter() {
        isHovered = true;
        GetComponentInChildren<SpriteRenderer>().color = hoveredColor;
    }

    private void OnMouseExit() {
        isHovered = false;
        GetComponentInChildren<SpriteRenderer>().color = unhoveredColor;
    }
}
