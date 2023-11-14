using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableBlock : MonoBehaviour
{
    GameObject mesh;
    bool hovered = false;
    bool selected = false;
    bool confirmed = false;
    public Material selectedMaterial;
    public Material unselectedMaterial;
    public Material confirmedMaterial;

    void Start()
    {
        mesh = transform.GetChild(0).gameObject;
        mesh.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(hovered)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(confirmed)
                {
                    selected = false;
                    confirmed = false;
                    EventManager.TriggerEvent("OnUnconfirmBlock", this.gameObject);
                    mesh.GetComponent<MeshRenderer>().material = unselectedMaterial;
                }
                if(selected)
                {
                    confirmed = true;
                    selected = false;
                    EventManager.TriggerEvent("OnConfirmBlock", this.gameObject);
                    mesh.GetComponent<MeshRenderer>().material = confirmedMaterial;
                } else
                {
                    selected = true;
                    mesh.GetComponent<MeshRenderer>().material = selectedMaterial;
                }
            }
        }
    }

    public void CantAddBlock()
    {
        confirmed = false;
        selected = false;
        mesh.GetComponent<MeshRenderer>().material = unselectedMaterial;
    }

    private void OnMouseEnter()
    {
        mesh.SetActive(true);
        hovered = true;
    }

    private void OnMouseExit()
    {
        if(!confirmed)
        {
            mesh.SetActive(false);
            selected = false;
            mesh.GetComponent<MeshRenderer>().material = unselectedMaterial;
        }
        hovered = false;
    }
}
