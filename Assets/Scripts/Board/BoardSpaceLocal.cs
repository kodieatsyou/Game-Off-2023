using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Will be attached to each space
///  This script is responsible for handling behavior for each board space.
///  Updates the spaces mesh to show the correct block
///  Handles selecting and mouse hovering
/// </summary>

public class BoardSpaceLocal: MonoBehaviour
{
    [SerializeField] private bool isBuilt;
    private Vector3 posInBoard;
    private Vector3 posInWorld;
    private float worldSpaceScalingFactor;
    private int valueOfNeighbors;
    private GameObject blockMesh;
    private GameObject detailMesh;
    private GameObject playerOnSpace;
    private BoardSpaceLocal blockBelow;
    private bool isSelectable;
    private bool isSelected;
    private bool isBeingHovered;
    private bool clicked;
    private BoxCollider bCollider;

    public BoardSpaceLocal InitializeSpace(Vector3 posInBoard, float worldSpaceScalingFactor, bool isBuilt)
    {
        //Set in-board and in-world positions
        this.posInBoard = posInBoard;
        this.worldSpaceScalingFactor = worldSpaceScalingFactor;
        posInWorld = new Vector3(posInBoard.x * worldSpaceScalingFactor, posInBoard.y * worldSpaceScalingFactor, posInBoard.z * worldSpaceScalingFactor);
        transform.position = posInWorld;

        //Set initial values for variables
        playerOnSpace = null;
        transform.name = ToString();
        this.isBuilt = isBuilt;
        valueOfNeighbors = 1;
        isSelectable = false;
        isSelected = false;
        isBeingHovered = false;
        clicked = false;

        //Set the meshes for the block, details, and cursors
        blockMesh = null;
        detailMesh = null;
        blockBelow = null;

        //Check for random detail and add it if needed
        System.Random rand = new System.Random();
        bool chanceForDetail = rand.Next(100) < 40;
        if (chanceForDetail && detailMesh == null)
        {
            detailMesh = Instantiate(GameAssets.i.block_details_[rand.Next(GameAssets.i.block_details_.Length - 1)], GetWorldPositionOfTopOfSpace(), Quaternion.Euler(new Vector3(0, rand.Next(360), 0)));
            detailMesh.transform.parent = transform;
            detailMesh.SetActive(false);
        }

        //Attach a collider to the block to handle clicking it
        if (GetComponent<BoxCollider>() == null)
        {
            bCollider = this.AddComponent<BoxCollider>();
            bCollider.size = new Vector3(2f, 2f, 2f);
            bCollider.center = new Vector3(0, 1.25f, 0);
            bCollider.enabled = false;
        }

        return this;
    }

    private void Update()
    {
        //Check if there is a block below and we havent saved it yet
        if(blockBelow == null)
        {
            if ((int)posInBoard.y - 1 >= 0 && BoardManagerLocal.BoardSpaceLocal_Arr[(int)posInBoard.x, (int)posInBoard.y - 1, (int)posInBoard.z] != null)
            {
                blockBelow = BoardManagerLocal.BoardSpaceLocal_Arr[(int)posInBoard.x, (int)posInBoard.y - 1, (int)posInBoard.z];
            }
        }

        //Calculate updated neighbor values
        if(CalculateValueOfNeighbors() != valueOfNeighbors)
        {
            valueOfNeighbors = CalculateValueOfNeighbors();
        }
        
        //Check if block below this one exists and is built if not set this block to not built
        if (blockBelow != null && !blockBelow.GetIsBuilt())
        {
            if(isBuilt)
            {
                isBuilt = false;
            }
        }

        if (isBuilt && (int)posInBoard.y > BoardManagerLocal.Instance.yOfCurrentHeighestBuiltBlock)
        {
            BoardManagerLocal.Instance.yOfCurrentHeighestBuiltBlock = (int)posInBoard.y;
        }

        UpdateBlockMesh();

        UpdateSelectability();

        HandleClick();
    }

    private void HandleClick()
    {
        if(isSelectable && isBeingHovered)
        {
            if (Input.GetMouseButtonDown(0))
            {
                clicked = true;
            }

            // Check if the right mouse button was just clicked (not held down indicating camera panning)
            if (clicked && !Input.GetMouseButton(0))
            {
                //isSelected = !isSelected;
                //SetIsBuiltPushUpdate(false);
                BoardManagerNetwork.Instance.BMPhotonView.RPC("RPCBoardManagerNetworkMovePlayerTo", Photon.Pun.RpcTarget.All, BoardManagerLocal.Instance.player.ActorNumber, posInBoard);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            clicked = false;
        }
    }

    private void UpdateBlockMesh()
    {
        if (isBuilt && valueOfNeighbors != 0)
        {
            // Store a reference to the blockMesh before destroying it
            GameObject previousBlockMesh = blockMesh?.gameObject;

            // Destroy the previous blockMesh
            Destroy(previousBlockMesh);

            // Check if the original object is not null before instantiating
            if (valueOfNeighbors % 11 == 0)
            {
                if(GameAssets.i.stone_blocks_[valueOfNeighbors / 11] != null)
                {
                    blockMesh = Instantiate(GameAssets.i.stone_blocks_[valueOfNeighbors / 11], transform);
                    detailMesh?.SetActive(false);
                }
               
            }
            else 
            {
                if (GameAssets.i.grass_blocks_[valueOfNeighbors] != null)
                {
                    blockMesh = Instantiate(GameAssets.i.grass_blocks_[valueOfNeighbors], transform);
                    detailMesh?.SetActive(true);
                }
            }
        }
        else
        {
            // Destroy the blockMesh if it exists
            Destroy(blockMesh?.gameObject);
            detailMesh?.SetActive(false);
        }
    }

    private int CalculateValueOfNeighbors()
    {
        int neighborValue = 1;
        int x = (int)posInBoard.x;
        int y = (int)posInBoard.y;
        int z = (int)posInBoard.z;

        if (z + 1 < BoardManagerLocal.Instance.BaseSize && BoardManagerLocal.BoardSpaceLocal_Arr[x, y, z + 1] != null)
        {
            if(BoardManagerLocal.BoardSpaceLocal_Arr[x, y, z + 1].GetIsBuilt()) neighborValue *= 2; // Front Block
        } 

        if (z - 1 >= 0 && BoardManagerLocal.BoardSpaceLocal_Arr[x, y, z - 1] != null)
        {
            if(BoardManagerLocal.BoardSpaceLocal_Arr[x, y, z - 1].GetIsBuilt()) neighborValue *= 3; // Behind Block
        }

        if (x + 1 < BoardManagerLocal.Instance.BaseSize && BoardManagerLocal.BoardSpaceLocal_Arr[x + 1, y, z] != null)
        {
            if(BoardManagerLocal.BoardSpaceLocal_Arr[x + 1, y, z].GetIsBuilt()) neighborValue *= 5; // Left Block
        } 

        if (x - 1 >= 0 && BoardManagerLocal.BoardSpaceLocal_Arr[x - 1, y, z] != null)
        {
            if(BoardManagerLocal.BoardSpaceLocal_Arr[x - 1, y, z].GetIsBuilt()) neighborValue *= 7; // Right Block
        }

        if (y + 1 < BoardManagerLocal.Instance.HeightSize && BoardManagerLocal.BoardSpaceLocal_Arr[x, y + 1, z] != null)
        {
            if(BoardManagerLocal.BoardSpaceLocal_Arr[x, y + 1, z].GetIsBuilt()) neighborValue *= 11; // Above Block
        }

        return neighborValue;
    }

    public void UpdateSelectability()
    {
        switch (BoardManagerLocal.Instance.selectionMode)
        {
            case SelectionMode.Build:
                if (!isBuilt && (blockBelow.GetIsBuilt() || blockBelow.GetIsSelected()))
                {
                    bCollider.enabled = true;
                    isSelectable = true;
                } else
                {
                    bCollider.enabled = false;
                    isSelectable = false;
                }
                break;
            case SelectionMode.Move:
                if(posInBoard.y == 0)
                {
                    bCollider.enabled = true;
                    isSelectable = true;
                }
                else
                {
                    bCollider.enabled = false;
                    isSelectable = false;
                }
                break;
        }
    }

    public override string ToString()
    {
        return $"Space: [{posInBoard.x},{posInBoard.y},{posInBoard.z}]";
    }

    public Vector3 GetPosInBoard() => posInBoard;

    public Vector3 GetPosInWorld() => posInWorld;

    public Vector3 GetWorldPositionOfTopOfSpace() => new Vector3(posInWorld.x, posInWorld.y + worldSpaceScalingFactor, posInWorld.z);

    public void SetWorldSpaceScalingFactor(float worldSpaceScalingFactor)
    {
        this.worldSpaceScalingFactor = worldSpaceScalingFactor;
        posInWorld = new Vector3(posInBoard.x * worldSpaceScalingFactor, posInBoard.y * worldSpaceScalingFactor, posInBoard.z * worldSpaceScalingFactor);
    }

    public float GetWorldSpaceScalingFactor() => worldSpaceScalingFactor;

    public void SetValueOfNeighbors(int valueOfNeighbors) => this.valueOfNeighbors = valueOfNeighbors;

    public int GetValueOfNeighbors() => valueOfNeighbors;

    public bool GetIsBuilt() => isBuilt;

    public bool GetIsSelected() {  return isSelected; }

    public bool GetIsBeingHovered() { return isBeingHovered; }

    public bool GetIsSelectable() {  return isSelectable; }

    public GameObject GetPlayerOnSpace() { return playerOnSpace; }

    public void PlacePlayerOnSpace(GameObject player)
    {
        this.playerOnSpace = player;
    }

    public void SetIsBuiltNoUpdate(bool isBuilt)
    {
        if (isBuilt != this.isBuilt)
        {
            this.isBuilt = isBuilt;
        }
    }

    public void SetIsBuiltPushUpdate(bool isBuilt) { 
        if(isBuilt != this.isBuilt)
        {
            this.isBuilt = isBuilt;
            if (BoardManagerLocal.Instance.initialized)
            {
                BoardManagerLocal.Instance.PushSyncFromLocalBoard(this);
            }
        }
    }

    private void OnMouseEnter()
    {
        if(isSelectable)
        {
            isBeingHovered = true;
        }
    }

    private void OnMouseExit()
    {
        isBeingHovered = false;
    }
}