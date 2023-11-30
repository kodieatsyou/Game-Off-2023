using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public delegate void PlayerClickedCallback(GameObject playerClicked);
public class PlayerClickOnHandler : MonoBehaviour
{

    public event PlayerClickedCallback OnPlayerClicked;

    bool isSelectable = false;
    bool isHovered = false;
    BoxCollider trigger;

    void Start() {
        trigger = GetComponent<BoxCollider>();
    }

    void Update() {
        if(isHovered && isSelectable) {
            if(Input.GetMouseButtonDown(0)) {
                OnPlayerClicked?.Invoke(gameObject);
            }
        }
    }
    public void ToggleSelectability(bool isSelectable) {
        this.isSelectable = false;
        trigger.enabled = isSelectable;
    }

    void OnMouseEnter() {
        isHovered = true;
    }

    void OnMouseExit() {
        isHovered = false;
    }
}
