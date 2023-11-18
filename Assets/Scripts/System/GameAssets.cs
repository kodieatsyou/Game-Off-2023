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
    [Header("Essentials")]
    public GameObject player_object_;
    [Header("Character")]
    public Texture[] character_skins_;
    [SerializedDictionary("Player Name", "Skin")]
    public SerializedDictionary<string, Texture> character_special_skins_;
    public Material super_special_material_;
    public GameObject[] character_head_accessories_;
    public GameObject[] character_face_accessories_;

    [Header("Board Blocks")]
    [Header("Components")]
    public GameObject hover_Object_;
    [Header("Buildable")]
    public GameObject buildable_block_;
    [Header("Grass")]
    [SerializedDictionary("Value", "Block Prefab")]
    public SerializedDictionary<int, GameObject> grass_blocks_;
    [Header("Stone")]
    [SerializedDictionary("Value", "Block Prefab")]
    public SerializedDictionary<int, GameObject> stone_blocks_;
    [Header("Detail")]
    public GameObject[] block_details_;

}