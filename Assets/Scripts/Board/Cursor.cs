using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    BoardSpace space;
    GameObject hover;
    GameObject indicator;
    GameObject moveArrow;
    GameObject buildScaffold;

    private void Start()
    {
        space = transform.parent.GetComponent<BoardSpace>();
        hover = transform.GetChild(0).gameObject;
        indicator = transform.GetChild(1).gameObject;
        buildScaffold = transform.GetChild(2).gameObject;
        moveArrow = transform.GetChild(3).gameObject;

        hover.SetActive(false);
        indicator.SetActive(false);
        buildScaffold.SetActive(false);
        moveArrow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(space.GetIsSelectable())
        {
            indicator.SetActive(true);
            if (space.GetIsBeingHovered())
            {
                hover.SetActive(true);
                indicator.SetActive(false);
                SetCursorMode();
            } else if(space.GetIsSelected())
            {
                hover.SetActive(false);
                indicator.SetActive(false);
                SetCursorMode();
            } else
            {
                hover.SetActive(false);
                indicator.SetActive(true);
                buildScaffold.SetActive(false);
                moveArrow.SetActive(false);
            }
        }
        
    }

    private void SetCursorMode()
    {
        switch (LocalBoardManager.Instance.selectionMode)
        {
            case SelectionMode.Move:
                buildScaffold.SetActive(false);
                moveArrow.SetActive(true);
                break;
            case SelectionMode.Build:
                buildScaffold.SetActive(true);
                moveArrow.SetActive(false);
                break;
        }
    }
}