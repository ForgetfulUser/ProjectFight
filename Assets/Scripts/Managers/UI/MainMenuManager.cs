using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
    public Transform MenuOptions_TRSFM;
    public Transform PlayerSelection_TRSFM;
    public List<Image> PlayerSelections_IMG = new List<Image>();
    public List<int> PlayerIDs = new List<int>();
    public Dictionary<int, int> playerIDs = new Dictionary<int, int>(); // Device | Player ID
    public List<int> DeviceIDs = new List<int>();
    private int currentID;
    public PlayerSelectionManager PlayerSelectionManager;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenPlayerSelection()
    {
        PlayerSelection_TRSFM.gameObject.SetActive(true);
    }
}