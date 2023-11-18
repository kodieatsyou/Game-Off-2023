using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Photon.Pun;

public class CharacterCreation : MonoBehaviour
{
    EventSystem eventSys;

    Vector3 mousePreviousPos = Vector3.zero;
    Vector3 mousePositionDelta = Vector3.zero;

    public GameObject characterPreview;
    public TMP_Text skinName;
    public TMP_InputField characterNameInput;
    public int currentTexture = 0;
    public int currentHeadAccessory = 0;
    public int currentFaceAccessory = 0;


    // Start is called before the first frame update
    void Start()
    {
        eventSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        characterPreview.GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_MainTex", GameAssets.i.character_skins_[currentTexture]);
        skinName.text = GameAssets.i.character_skins_[currentTexture].name;
    }

    // Update is called once per frame
    /// <summary>
    /// Check if we are hovering over the character preview drag location. 
    /// Check if the mouse is clicked down and allow the preview to rotate with the mouse
    /// </summary>
    void Update()
    {
        if(IsPointerOverGameObjectName("Character Drag Location"))
        {
            if (Input.GetMouseButton(0))
            {
                mousePositionDelta = (Input.mousePosition - mousePreviousPos);
                characterPreview.transform.Rotate(transform.up, -Vector3.Dot(mousePositionDelta, Camera.main.transform.right), Space.World);
            }
            mousePreviousPos = Input.mousePosition;
        }    

        ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable();
        initialProps["texture"] = currentTexture;
        initialProps["head"] = currentHeadAccessory;
        initialProps["face"] = currentFaceAccessory;
        PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);

        // Debug.LogWarning(string.Format("Character Creation ---- {0}-texture", PhotonNetwork.NickName)); 
    }

    /// <summary>
    /// Is the pointer hovering over a game object with the given name
    /// </summary>
    /// <param name="name">Name of the game object hovering over</param>
    /// <returns>Status</returns>
    private bool IsPointerOverGameObjectName(string name)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0)
        {
            foreach (var go in raycastResults)
            {
                if (go.gameObject.name == name)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Set the character preview's material texture to the next one in the list from game assets
    /// </summary>
    /// <returns>None</returns>
    public void NextSkin()
    {
        if(currentTexture + 1 >= GameAssets.i.character_skins_.Length)
        {
            currentTexture = 0;
        } else
        {
            currentTexture += 1;
        }

        characterPreview.GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[currentTexture]);
        skinName.text = GameAssets.i.character_skins_[currentTexture].name;
    }

    /// <summary>
    /// Set the character preview's material texture to the previouse one in the list from game assets
    /// </summary>
    /// <returns>None</returns>
    public void PreviousSkin()
    {
        if (currentTexture - 1 < 0)
        {
            currentTexture = GameAssets.i.character_skins_.Length - 1;
        }
        else
        {
            currentTexture -= 1;
        }

        characterPreview.GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[currentTexture]);
        skinName.text = GameAssets.i.character_skins_[currentTexture].name;
    }

    /// <summary>
    /// Check what text is currently in the name input field and see if it matches any keys in the special skins dictionary.
    /// If it does, change the texture on the character preview material to the special texture.
    /// </summary>
    /// <returns>None</returns>
    public void CheckForSpecialSkin()
    {
        string currentName = characterNameInput.text;
        if (GameAssets.i.character_special_skins_.ContainsKey(currentName))
        {
            characterPreview.GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_special_skins_[currentName]);
            skinName.text = GameAssets.i.character_special_skins_[currentName].name;
        }
        else
        {
            characterPreview.GetComponentInChildren<SkinnedMeshRenderer>().material.SetTexture("_BaseMap", GameAssets.i.character_skins_[currentTexture]);
            skinName.text = GameAssets.i.character_skins_[currentTexture].name;
        }
    }

    /// <summary>
    /// Instantiate the next head accessory from the list in game objects and parent it to the character preview's head accessory slot
    /// </summary>
    /// <returns>None</returns>
    public void NextHeadAccessory()
    {
        currentHeadAccessory += 1;
        //Destory current head accessory if there is one
        Transform accessorySlot = characterPreview.transform.Find("Armature/body/neck/head/head_end/Head Accessory");
        if (accessorySlot.childCount == 1)
        {
            Destroy(accessorySlot.GetChild(0).gameObject);
        }
        if(currentHeadAccessory - 1 >= GameAssets.i.character_head_accessories_.Length)
        {
            //Loop our accessory to 0 and return since we dont need to instantiate anything
            currentHeadAccessory = 0;
            return;
        }
        //Instantiate head accessory
        Instantiate(GameAssets.i.character_head_accessories_[currentHeadAccessory - 1], accessorySlot);
    }

    /// <summary>
    /// Instantiate the previous head accessory from the list in game objects and parent it to the character preview's head accessory slot
    /// </summary>
    /// <returns>None</returns>
    public void PreviousHeadAccessory()
    {
        currentHeadAccessory -= 1;
        //Destory current head accessory if there is one
        Transform accessorySlot = characterPreview.transform.Find("Armature/body/neck/head/head_end/Head Accessory");
        if (accessorySlot.childCount == 1)
        {
            Destroy(accessorySlot.GetChild(0).gameObject);
        }
        if (currentHeadAccessory == 0)
        {
            return;
        }

        if (currentHeadAccessory - 1 < -1)
        {
            currentHeadAccessory = GameAssets.i.character_head_accessories_.Length;
        }
        //Instantiate head accessory
        Instantiate(GameAssets.i.character_head_accessories_[currentHeadAccessory - 1], accessorySlot);
    }

    /// <summary>
    /// Instantiate the next face accessory from the list in game objects and parent it to the character preview's head accessory slot
    /// </summary>
    /// <returns>None</returns>
    public void NextFaceAccessory()
    {
        currentFaceAccessory += 1;
        //Destory current face accessory if there is one
        Transform accessorySlot = characterPreview.transform.Find("Armature/body/neck/head/Face Accessory");
        if (accessorySlot.childCount == 1)
        {
            Destroy(accessorySlot.GetChild(0).gameObject);
        }
        if (currentFaceAccessory - 1 >= GameAssets.i.character_face_accessories_.Length)
        {
            //Loop our accessory to 0 and return since we dont need to instantiate anything
            currentFaceAccessory = 0;
            return;
        }
        //Instantiate face accessory
        Instantiate(GameAssets.i.character_face_accessories_[currentFaceAccessory - 1], accessorySlot);
    }

    /// <summary>
    /// Instantiate the previous face accessory from the list in game objects and parent it to the character preview's head accessory slot
    /// </summary>
    /// <returns>None</returns>
    public void PreviousFaceAccessory()
    {
        currentFaceAccessory -= 1;
        //Destory current face accessory if there is one
        Transform accessorySlot = characterPreview.transform.Find("Armature/body/neck/head/Face Accessory");
        if (accessorySlot.childCount == 1)
        {
            Destroy(accessorySlot.GetChild(0).gameObject);
        }
        if (currentFaceAccessory == 0)
        {
            return;
        }

        if (currentFaceAccessory - 1 < -1)
        {
            currentFaceAccessory = GameAssets.i.character_face_accessories_.Length;
        }
        //Instantiate face accessory
        Instantiate(GameAssets.i.character_face_accessories_[currentFaceAccessory - 1], accessorySlot);
    }
       
}
