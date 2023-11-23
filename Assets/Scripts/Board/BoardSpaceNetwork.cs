using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardSpaceNetwork
{
    public bool isBuilt;
    public Vector3 posInBoard;
    public Vector3 posInWorld;
    public float worldSpaceScalingFactor;
    public int playerIDOnSpace;

    // Constructor
    public BoardSpaceNetwork(Vector3 posInBoard, float worldSpaceScalingFactor, bool isBuilt)
    {
        this.isBuilt = isBuilt;
        this.worldSpaceScalingFactor = worldSpaceScalingFactor;
        this.posInBoard = posInBoard;
    }

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
