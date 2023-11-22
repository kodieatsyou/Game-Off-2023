using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Will be attached to each space
///  This script is responsible for handling behavior for each board space.
///  Updates the spaces mesh to show the correct block
///  Handles selecting and mouse hovering
/// </summary>

public class BoardSpace : MonoBehaviour
{
    [SerializeField] private bool isBuilt;
    private bool wasBuiltLastFrame;
    private bool needsUpdate;
    private Vector3 posInBoard;
    private Vector3 posInWorld;
    private float worldSpaceScalingFactor;
    private int valueOfNeighbors;
    private GameObject blockMesh;
    private GameObject detailMesh;
    private GameObject cursorMesh;
    private GameObject playerOnSpace;
    private BoardSpace blockBelow;
    private bool isSelectable;
    private bool isSelected;

    public BoardSpace InitializeSpace(Vector3 posInBoard, float worldSpaceScalingFactor, bool isBuilt)
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
        wasBuiltLastFrame = isBuilt;
        valueOfNeighbors = 1;
        isSelectable = false;
        isSelected = false;
        needsUpdate = true;

        //Set the meshes for the block, details, and cursors
        blockMesh = null;
        detailMesh = null;
        cursorMesh = null;
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
            BoxCollider c = this.AddComponent<BoxCollider>();
            c.size = new Vector3(2.7f, 2.7f, 2.7f);
            c.center = new Vector3(0, 1.25f, 0);
            c.enabled = false;
        }

        return this;
    }

    private void Update()
    {
        //Check if isBuilt was changed from last frame
        if(isBuilt != wasBuiltLastFrame){ needsUpdate = true; }

        //Check if there is a block below and we havent saved it yet
        if(blockBelow == null)
        {
            if ((int)posInBoard.y - 1 > 0 && BoardManager.BoardSpace_Arr[(int)posInBoard.x, (int)posInBoard.y - 1, (int)posInBoard.z] != null)
            {
                blockBelow = BoardManager.BoardSpace_Arr[(int)posInBoard.x, (int)posInBoard.y - 1, (int)posInBoard.z];
            }
        }

        //Calculate updated neighbor values
        if(CalculateValueOfNeighbors() != valueOfNeighbors)
        {
            valueOfNeighbors = CalculateValueOfNeighbors();
            needsUpdate = true;
        }
        
        //Check if block below this one exists and is built if not set this block to not built
        if (blockBelow != null && !blockBelow.GetIsBuilt())
        {
            if(isBuilt)
            {
                isBuilt = false;
                needsUpdate = true;
            }
        }
        
        //If we need to update then we update the block mesh
        if(needsUpdate)
        {
            UpdateBlockMesh();
            needsUpdate = false;
        }

        wasBuiltLastFrame = isBuilt;
    }

    private void UpdateBlockMesh()
    {
        if (isBuilt && valueOfNeighbors != 0)
        {
            if (valueOfNeighbors % 11 == 0)
            {
                Destroy(blockMesh?.gameObject);
                blockMesh = Instantiate(GameAssets.i.stone_blocks_[valueOfNeighbors / 11], transform);
                detailMesh?.SetActive(false);
            }
            else
            {
                Destroy(blockMesh?.gameObject);
                blockMesh = Instantiate(GameAssets.i.grass_blocks_[valueOfNeighbors], transform);
                detailMesh?.SetActive(true);
            }
        } else
        {
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

        if (z + 1 < BoardManager.Instance.BaseSize && BoardManager.BoardSpace_Arr[x, y, z + 1] != null)
        {
            if(BoardManager.BoardSpace_Arr[x, y, z + 1].GetIsBuilt()) neighborValue *= 2; // Front Block
        } 

        if (z - 1 >= 0 && BoardManager.BoardSpace_Arr[x, y, z - 1] != null)
        {
            if(BoardManager.BoardSpace_Arr[x, y, z - 1].GetIsBuilt()) neighborValue *= 3; // Behind Block
        }

        if (x + 1 < BoardManager.Instance.BaseSize && BoardManager.BoardSpace_Arr[x + 1, y, z] != null)
        {
            if(BoardManager.BoardSpace_Arr[x + 1, y, z].GetIsBuilt()) neighborValue *= 5; // Left Block
        } 

        if (x - 1 >= 0 && BoardManager.BoardSpace_Arr[x - 1, y, z] != null)
        {
            if(BoardManager.BoardSpace_Arr[x - 1, y, z].GetIsBuilt()) neighborValue *= 7; // Right Block
        }

        if (y + 1 < BoardManager.Instance.HeightSize && BoardManager.BoardSpace_Arr[x, y + 1, z] != null)
        {
            if(BoardManager.BoardSpace_Arr[x, y + 1, z].GetIsBuilt()) neighborValue *= 11; // Above Block
        }

        return neighborValue;
    }

    public void UpdateSelectability()
    {
        switch (BoardManager.Instance.selectionMode)
        {
            case SelectionMode.Build:
                if (!isBuilt && (blockBelow.GetIsBuilt() || blockBelow.GetIsSelected()))
                {
                    isSelectable = true;
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

}