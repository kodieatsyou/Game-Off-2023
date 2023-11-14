using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestingController : MonoBehaviour
{
    public List<GameObject> confirmedSpots;

    void OnEnable()
    {
        EventManager.StartListening("OnConfirmBlock", AddBlockToConfirmedList);
        EventManager.StartListening("OnUnconfirmBlock", RemoveBlockFromConfirmedList);
    }

    void OnDisable()
    {
        EventManager.StopListening("OnConfirmBlock", AddBlockToConfirmedList);
        EventManager.StopListening("OnUnconfirmBlock", RemoveBlockFromConfirmedList);
    }


    // Start is called before the first frame update
    void Start()
    {
        confirmedSpots = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceChar()
    {
        if(confirmedSpots.Count > 0)
        {
            Instantiate(GameAssets.i.player_object_, confirmedSpots[0].transform.position, Quaternion.identity);
        }
    }

    public void AddBlockToConfirmedList(GameObject caller)
    {
        Debug.Log("Trying to add: " + caller.name);
        if(confirmedSpots.Count >= 2)
        {
            caller.GetComponent<BuildableBlock>().CantAddBlock();
        } else
        {
            confirmedSpots.Add(caller);
        }
    }

    public void RemoveBlockFromConfirmedList(GameObject caller)
    {
        confirmedSpots.Remove(caller);
    }
}
