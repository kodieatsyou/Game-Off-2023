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
    public GameObject board_;
    public GameObject wind_button_;
    public GameObject wind_particle_;
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
    [Header("Cards")]
    public GameObject card_;
    public SerializedDictionary<CardType, string> card_descriptions_;
    public SerializedDictionary<CardType, Sprite> card_art_;


    [Header("Player")]
    public GameObject player_object_;
    public Texture[] character_skins_;
    [SerializedDictionary("Player Name", "Skin")]
    public Texture[] character_special_skins_;
    public Material super_special_material_;
    public GameObject[] character_head_accessories_;
    public GameObject[] character_face_accessories_;

    [Header("Animation Props")]
    public GameObject prop_grapple_gun_;
    public GameObject prop_hourglass_;


    [Header("Card Particles")]
    public GameObject card_switch_particle_;
}
