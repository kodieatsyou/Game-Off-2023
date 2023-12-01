using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public delegate void PlayerClickedCallback(GameObject playerClicked);
public class PlayerClickOnHandler : MonoBehaviour
{

    public event PlayerClickedCallback OnPlayerClicked;

    public GameObject highlightBox;

    public bool isSelectable = false;
    public bool isHovered = false;
    BoxCollider trigger;

    CardType cardClickType;

    void Start() {
        trigger = GetComponent<BoxCollider>();
        ToggleSelectability(false);
    }

    void Update() {
        if(isHovered && isSelectable) {
            if(Input.GetMouseButtonDown(0)) {
                OnPlayerClicked?.Invoke(gameObject);
            }
        }
    }
    public void ToggleSelectability(bool isSelectable) {
        this.isSelectable = isSelectable;
        trigger.enabled = isSelectable;
        if(!isSelectable)
        {
            highlightBox.SetActive(false);
        }

    }

    void OnMouseEnter() {
        isHovered = true;
        highlightBox.SetActive(true);
    }

    void OnMouseExit() {
        isHovered = false;
        highlightBox.SetActive(false);
    }
}
