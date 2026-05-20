using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        LevelManager.Instance.playerIDs = playerIDs;
        LevelManager.Instance.LoadNextScene();
    }

    public void SelectPlayer(PlayerInput playerInput)
    {
        for (int i = 0; i < playerInput.devices.Count; i++)
        {
            if (DeviceIDs.Contains(playerInput.devices[i].deviceId)) return;
        }

        int deviceID = playerInput.devices[0].deviceId;
        playerIDs.Add(playerIDs.Count,  deviceID);
        DeviceIDs.Add(deviceID);
        PlayerSelections_IMG[currentID].color = Color.white;
        currentID++;
    }
}