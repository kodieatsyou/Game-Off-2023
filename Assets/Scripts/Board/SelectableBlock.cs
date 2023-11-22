using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectableBlock : MonoBehaviour
{
    GameObject cursor;

    bool isSelectable = false;
    bool isSelected = false;
    bool hovering = false;
    bool clicked = false;

    // Start is called before the first frame update
    void Start()
    {
        if(GetComponent<BoxCollider>() == null)
        {
            BoxCollider c = this.AddComponent<BoxCollider>();
            c.size = new Vector3(2.7f, 2.7f, 2.7f);
            c.center = new Vector3(0, 1.25f, 0);
        }
    }

    public void SetIsSelectable(bool isSelectable)
    {
        this.isSelectable = isSelectable;
    }

    public void SetIsSelected()
    {
        isSelected = true;
    }

    public void ClearSelection()
    {
        this.isSelected = false;
    }

    private void Update()
    {
        GetComponent<BoxCollider>().enabled = isSelectable;
        if (hovering)
        {
            if (cursor == null)
            {
                cursor = Instantiate(GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>().GetBlockCursor(), transform, false);
            }

            //Show the hover part of cursor
            cursor.transform.GetChild(1).gameObject.SetActive(true);

            if (isSelectable)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    clicked = true;
                }

                // Check if the right mouse button was just clicked (not held down indicating camera panning)
                if (clicked && !Input.GetMouseButton(0))
                {
                    isSelected = !isSelected;
                    Debug.Log("Selecting block!");
                }
            }
        }  
        else
        {
            if (cursor != null)
            {
                if(!isSelected) 
                {
                    Destroy(cursor.gameObject);
                } else
                {
                    //Hide the hover part of cursor
                    cursor.transform.GetChild(1).gameObject.SetActive(false);
                }
                
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            clicked = false;
        }

    }

    private void OnMouseEnter()
    {
        hovering = true;
    }

    private void OnMouseExit()
    {
        hovering = false;
    }
}
