using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    GameObject hoverObj;

    BoardSpace boardSpace;

    // Start is called before the first frame update
    void Start()
    {
        if(this.GetComponent<BoxCollider>() == null)
        {
            BoxCollider c = this.AddComponent<BoxCollider>();
            c.size = new Vector3(2.7f, 2.7f, 2.7f);
            c.center = new Vector3(0, 1.25f, 0);
        }
        hoverObj = Instantiate(GameAssets.i.hover_Object_, this.transform, false);
        hoverObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(hoverObj.activeSelf)
        {
            if(Input.GetMouseButtonDown(0))
            {
                EventManager.TriggerEvent("onMovePlayer", this.gameObject);
            }
        }
    }

    private void OnMouseEnter()
    {
       if(hoverObj != null)
        {
            hoverObj.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (hoverObj != null)
        {
            hoverObj.SetActive(false);
        }
    }

    public void SetSpaceObj(BoardSpace spaceObj)
    {
        boardSpace = spaceObj;
    }

    public BoardSpace GetBoardSpace() { return this.boardSpace; }
}
