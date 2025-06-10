using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillCount : MonoBehaviour, IDataPersistence
{
    private TextMeshProUGUI killCountText;
    
    int killCount;

    private void Awake()
    {
        killCountText = this.GetComponent<TextMeshProUGUI>();
    }

    public void LoadData(GameData data)
    {
        this.killCount = data.killCount;
    }

    public void SaveData(ref GameData data)
    {
        data.killCount = this.killCount;
    }

    public void AddKill()
    {
        killCount++;

        killCountText.text = "Kills : " + killCount;
    }
}
