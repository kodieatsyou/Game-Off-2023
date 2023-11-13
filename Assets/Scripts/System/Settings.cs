using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Settings")]
public class Settings : ScriptableObject
{
    public int boardSize;
    public int boardHeight;
    public int maxPlayers;
    public float cameraDistanceOffset;
    public float cameraAngle;
}
