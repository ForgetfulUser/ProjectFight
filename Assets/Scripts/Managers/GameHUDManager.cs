using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameHUDManager : MonoBehaviour
{
    public List<Player> Players = new List<Player>();
    public TMP_Text Timer_TXT;
    public GameObject GameOver_GO;

    public void StartHUD(List<Player> players)
    {
        Players = players;

    }

    public void UpdateHUD(int minutes, float seconds)
    {
        int secs = Mathf.RoundToInt(seconds);
        if (secs == 60) secs = 59; 
        if(minutes < 0) minutes = 0;
        Timer_TXT.text = minutes + ":" + secs;
    }
    public void GameOver()
    {
        GameOver_GO.SetActive(true);
    }
}
