using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCanvasManager : MonoBehaviour
{
    [Header("References")]
    public Image NameBackground_IMG;
    public SpriteRenderer Arrow_IMG;
    public TMP_Text PlayerName_TXT;

    [Header("Color")]
    [Range(0, 1)] public float BackgroundTransparency;
    public List<Color> LightColors;
    public List<Color> DarkColors;
    
    public void StartManager(int playerIndex)
    {
        // Setup
        Color lightColor = LightColors[playerIndex];
        Color transLight = LightColors[playerIndex];
        transLight.a = BackgroundTransparency;
        Color darkColor = DarkColors[playerIndex];
        Color transDark = DarkColors[playerIndex];
        transDark.a = BackgroundTransparency;

        // Apply
        NameBackground_IMG.color = transLight;
        Arrow_IMG.color = lightColor;
        PlayerName_TXT.color = lightColor;
        PlayerName_TXT.text = "P" + (playerIndex + 1);
    }
}
