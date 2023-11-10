using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;

    public static GameAssets i
    {
        get
        {
            if (_i == null) _i = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            return _i;
        }
    }

    [Header("Loadable Assets")]
    [Header("Board Blocks")]
    [Header("Grass")]
    [SerializedDictionary("Value", "Block Prefab")]
    public SerializedDictionary<int, GameObject> grass_blocks_;

    [Header("Stone")]
    [SerializedDictionary("Value", "Block Prefab")]
    public SerializedDictionary<int, GameObject> stone_blocks_;

    [Header("Detail")]
    public GameObject[] block_details_;

}
