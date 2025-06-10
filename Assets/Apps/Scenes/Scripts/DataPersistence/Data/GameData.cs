using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int killCount;
    public Vector3 playerPosition;

    public GameData()
    {
        //default values
        
        this.killCount = 0;
        playerPosition = Vector3.zero;
    }
}
