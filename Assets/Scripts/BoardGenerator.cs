using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public int MinCount = 50;
    public int MaxCount = 100;
    public GameObject Board;

    // Start is called before the first frame update
    void Start()
    {
        GameObject boardPiece = Board.transform.GetChild(0).gameObject;
        int num_created = Generate(boardPiece);

        Console.WriteLine("Created board of ", num_created, " pieces");
    }

    int Generate(GameObject parentPiece)
    {
        System.Random randGen = new System.Random();

        int targetCount = randGen.Next(MinCount, MaxCount); // Random number somewhere between MinCount and MaxCount

        for (int i = 0; i < targetCount; i++)
        {
            GameObject clone = Instantiate(parentPiece, new Vector3(i * 2.0f, 0, 0), Quaternion.identity);
        }

        return targetCount;
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
