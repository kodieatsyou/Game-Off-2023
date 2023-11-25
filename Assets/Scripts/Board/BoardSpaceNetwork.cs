using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardSpaceNetwork
{
    public bool isBuilt;
    public Vector3 posInBoard;
    public Vector3 posInWorld;
    public int playerIDOnSpace;

    public BoardSpaceNetwork(Vector3 posInBoard, bool isBuilt)
    {
        this.isBuilt = isBuilt;
        posInWorld = new Vector3(posInBoard.x * 2.5f, posInBoard.y * 2.5f, posInBoard.z * 2.5f);
        this.posInBoard = posInBoard;
        this.playerIDOnSpace = -1;
    }

    public BoardSpaceNetwork(string json)
    {
        this.isBuilt = FromJson(json).isBuilt;
        this.posInBoard = FromJson(json).posInBoard;
        this.posInWorld = FromJson(json).posInWorld;
        this.playerIDOnSpace = FromJson(json).playerIDOnSpace;
    }

    public Vector3 GetWorldPositionOfTopOfSpace() => new Vector3(posInWorld.x, posInWorld.y + 2.5f, posInWorld.z);

    // Method to serialize the class to JSON
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    // Method to create an instance from a JSON string
    public static BoardSpaceNetwork FromJson(string json)
    {
        return JsonUtility.FromJson<BoardSpaceNetwork>(json);
    }
}
