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

    [Header("Board")]
    public GameObject board_space_object_;
    [Header("Cursors")]
    public GameObject board_cursor_;
    [Header("Grass")]
    [SerializedDictionary("Value", "Block Prefab")]
    public SerializedDictionary<int, GameObject> grass_blocks_;
    [Header("Stone")]
    [SerializedDictionary("Value", "Block Prefab")]
    public SerializedDictionary<int, GameObject> stone_blocks_;
    [Header("Detail")]
    public GameObject[] block_details_;

    [Header("Loadable Assets")]
    [Header("Essentials")]
    public GameObject player_object_;
    [Header("Character")]
    public Texture[] character_skins_;
    [SerializedDictionary("Player Name", "Skin")]
    public Texture[] character_special_skins_;
    public Material super_special_material_;
    public GameObject[] character_head_accessories_;
    public GameObject[] character_face_accessories_;
}
