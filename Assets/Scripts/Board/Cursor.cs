using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    BoardSpace space;
    public GameObject blockHover;
    GameObject playerHover;

    GameObject currentHover;
    GameObject indicator;
    GameObject moveArrow;
    GameObject buildScaffold;

    private void Start()
    {
        space = transform.parent.GetComponent<BoardSpace>();
        blockHover = transform.GetChild(0).gameObject;
        playerHover = transform.GetChild(1).gameObject;
        indicator = transform.GetChild(2).gameObject;
        buildScaffold = transform.GetChild(3).gameObject;
        moveArrow = transform.GetChild(4).gameObject;

        currentHover = blockHover;

        blockHover.SetActive(false);
        playerHover.SetActive(false);
        indicator.SetActive(false);
        buildScaffold.SetActive(false);
        moveArrow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        indicator.SetActive(false);
        currentHover.SetActive(false);
        buildScaffold.SetActive(false);
        moveArrow.SetActive(false);

        if (space.GetIsSelectable())
        {
            indicator.SetActive(true);
            if (space.GetIsBeingHovered())
            {
                currentHover.SetActive(true);
                indicator.SetActive(false);
                SetCursorMode();
            } else if(space.GetIsSelected())
            {
                currentHover.SetActive(false);
                indicator.SetActive(false);
                SetCursorMode();
            } else
            {
                currentHover.SetActive(false);
                indicator.SetActive(true);
                buildScaffold.SetActive(false);
                moveArrow.SetActive(false);
            }
        }
        
    }

    private void SetCursorMode()
    {
        switch (Board.Instance.selectionMode)
        {
            case SelectionMode.Move:
                buildScaffold.SetActive(false);
                moveArrow.SetActive(true);
                currentHover = blockHover;
                indicator.transform.position = new Vector3(indicator.transform.position.x, 3.75f, indicator.transform.position.z);
                break;
            case SelectionMode.Grapple:
                buildScaffold.SetActive(false);
                moveArrow.SetActive(true);
                currentHover = blockHover;
                indicator.transform.position = new Vector3(indicator.transform.position.x, 3.75f, indicator.transform.position.z);
                break;
            case SelectionMode.Build:
                buildScaffold.SetActive(true);
                moveArrow.SetActive(false);
                indicator.transform.position = new Vector3(indicator.transform.position.x, 1.25f, indicator.transform.position.z);
                break;
            case SelectionMode.Ninja:
                buildScaffold.SetActive(false);
                moveArrow.SetActive(true);
                currentHover = blockHover;
                indicator.transform.position = new Vector3(indicator.transform.position.x, 3.75f, indicator.transform.position.z);
                break;
            case SelectionMode.None:
                currentHover = blockHover;
                break;
        }
    }
}