using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDManager : MonoBehaviour
{
    public List<Player> Players = new List<Player>();
    public List<Sprite> LifeSprites = new List<Sprite>();
    public List<Image> PlayerLivesSpriteRender = new List<Image>();
    public TMP_Text Timer_TXT;
    public GameObject GameOver_GO;

    public void StartHUD(List<Player> players)
    {
        Players = players;
    }

    public void UpdateHUD(int minutes, float seconds)
    {
        UpdateTimer(minutes, seconds);

        UpdatePlayerHealLives();
    }

    public void UpdateTimer(int minutes, float seconds)
    {
        int secs = Mathf.RoundToInt(seconds);
        if (secs == 60 && minutes > 0) secs = 59;
        if (minutes < 0) minutes = 0;
        string text = minutes + ":";
        if (secs < 10) text += "0" + secs;
        else text += secs;
        Timer_TXT.text = text;
    }

    public void UpdatePlayerHealLives()
    {
        for (int i = 0;  i < PlayerLivesSpriteRender.Count; i++)
        {
            if(i >= Players.Count)
            {
                PlayerLivesSpriteRender[i].transform.parent.gameObject.SetActive(false);
                continue;
            }
            int lifeIndex = Players[i].CurrentLives;
            if(lifeIndex < 0) lifeIndex = 0;
            PlayerLivesSpriteRender[i].sprite = LifeSprites[lifeIndex];
        }
    }

    public void GameOver()
    {
        GameOver_GO.SetActive(true);
    }
}