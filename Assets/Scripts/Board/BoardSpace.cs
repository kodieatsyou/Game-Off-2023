using System;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    [SerializeField] private bool isBuilt;
    private GameObject spaceObject;
    private Vector3 posInBoard;
    private Vector3 posInWorld;
    private float worldSpaceScalingFactor;
    private int valueOfNeighbors;
    private GameObject blockMesh;
    private GameObject detailMesh;
    private GameObject playerOnSpace;

    public BoardSpace InitializeSpace(Vector3 posInBoard, float worldSpaceScalingFactor, bool isBuilt)
    {
        this.posInBoard = posInBoard;
        this.worldSpaceScalingFactor = worldSpaceScalingFactor;
        this.isBuilt = isBuilt;
        posInWorld = new Vector3(posInBoard.x * worldSpaceScalingFactor, posInBoard.y * worldSpaceScalingFactor, posInBoard.z * worldSpaceScalingFactor);
        transform.position = posInWorld;
        valueOfNeighbors = 0;
        blockMesh = null;
        detailMesh = null;
        playerOnSpace = null;
        name = ToString();
        System.Random rand = new System.Random();
        bool chanceForDetail = rand.Next(100) < 40;
        if (chanceForDetail && detailMesh == null)
        {
            SetDetailMesh(GameAssets.i.block_details_[rand.Next(GameAssets.i.block_details_.Length - 1)]);
        }
        return this;
    }

    public override string ToString()
    {
        return $"Space: [{posInBoard.x},{posInBoard.y},{posInBoard.z}]";
    }

    public void UpdateSpace()
    {
        bool blockBelowIsBuilt = (int)posInBoard.y - 1 >= 0 && BoardManagerNew.BoardSpace_Arr[(int)posInBoard.x, (int)posInBoard.y - 1, (int)posInBoard.z].GetIsBuilt();

        if ((int)posInBoard.y - 1 >= 0 && !blockBelowIsBuilt)
        {
            isBuilt = false;
        }

        valueOfNeighbors = CalculateValueOfNeighbors();

        if (isBuilt)
        {
            SetBlockMeshBasedOnNeighbors();
        }
        else
        {
            if (detailMesh != null) detailMesh.SetActive(false);
            Destroy(blockMesh?.gameObject);
        }
    }

    public void SetSpaceObject(GameObject spaceObject) => this.spaceObject = spaceObject;

    public GameObject GetSpaceObject() => spaceObject;

    public void SetPosInBoard(Vector3 posInBoard)
    {
        this.posInBoard = posInBoard;
        posInWorld = new Vector3(posInBoard.x * worldSpaceScalingFactor, posInBoard.y * worldSpaceScalingFactor, posInBoard.z * worldSpaceScalingFactor);
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

    public void SetBlockMesh(GameObject blockMesh)
    {
        Destroy(this.blockMesh?.gameObject);
        this.blockMesh = Instantiate(blockMesh, transform);
    }

    public GameObject GetBlockMesh() => blockMesh;

    public void SetDetailMesh(GameObject detailMesh)
    {
        Destroy(this.detailMesh?.gameObject);
        System.Random rand = new System.Random();
        this.detailMesh = Instantiate(detailMesh, GetWorldPositionOfTopOfSpace(), Quaternion.Euler(new Vector3(0, rand.Next(360), 0)));
        this.detailMesh.transform.parent = transform;
        this.detailMesh.SetActive(false);
    }

    public bool GetIsBuilt() => isBuilt;

    public void SetIsBuilt(bool isBuilt) => this.isBuilt = isBuilt;

    public bool GetIsBuildable()
    {
        bool blockBelowIsBuilt = (int)posInBoard.y - 1 >= 0 && BoardManagerNew.BoardSpace_Arr[(int)posInBoard.x, (int)posInBoard.y - 1, (int)posInBoard.z].GetIsBuilt();

        if (blockBelowIsBuilt && !isBuilt)
        {
            return true;
        }

        return false;
    }

    public bool GetCanMoveToFromPos(Vector3 posInBoardToMoveFrom) {
        bool blockAboveIsBuilt = (int)posInBoard.y + 1 < BoardManagerNew.Instance.HeightSize && BoardManagerNew.BoardSpace_Arr[(int)posInBoard.x, (int)posInBoard.y + 1, (int)posInBoard.z].GetIsBuilt();
        if(!blockAboveIsBuilt)
        {
            return posInBoardToMoveFrom.y == posInBoard.y;
        } else
        {
            return false;
        }
        
    }

    private void SetBlockMeshBasedOnNeighbors()
    {
        if (valueOfNeighbors % 11 == 0)
        {
            SetBlockMesh(GameAssets.i.stone_blocks_[valueOfNeighbors / 11]);
            if (detailMesh != null) detailMesh.SetActive(false);
        }
        else
        {
            SetBlockMesh(GameAssets.i.grass_blocks_[valueOfNeighbors]);
            if (detailMesh != null) detailMesh.SetActive(true);
        }
    }

    private int CalculateValueOfNeighbors()
    {
        int neighborValue = 1;
        int x = (int)posInBoard.x;
        int y = (int)posInBoard.y;
        int z = (int)posInBoard.z;

        if (z + 1 < BoardManagerNew.Instance.BaseSize && BoardManagerNew.BoardSpace_Arr[x, y, z + 1].GetIsBuilt())
            neighborValue *= 2; // Front Block

        if (z - 1 >= 0 && BoardManagerNew.BoardSpace_Arr[x, y, z - 1].GetIsBuilt())
            neighborValue *= 3; // Behind Block

        if (x + 1 < BoardManagerNew.Instance.BaseSize && BoardManagerNew.BoardSpace_Arr[x + 1, y, z].GetIsBuilt())
            neighborValue *= 5; // Left Block

        if (x - 1 >= 0 && BoardManagerNew.BoardSpace_Arr[x - 1, y, z].GetIsBuilt())
            neighborValue *= 7; // Right Block

        if (y + 1 < BoardManagerNew.Instance.HeightSize && BoardManagerNew.BoardSpace_Arr[x, y + 1, z].GetIsBuilt())
            neighborValue *= 11; // Above Block

        return neighborValue;
    }
}