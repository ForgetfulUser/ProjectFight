using System.Collections.Generic;
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
    public List<int> DeviceIDs = new List<int>();
    private int currentID;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        LevelManager.Instance.PlayerIDs = PlayerIDs;
        LevelManager.Instance.LoadNextScene();
    }

    public void SelectPlayer(PlayerInput playerInput)
    {
        int deviceID = playerInput.devices[0].deviceId;
        if (DeviceIDs.Contains(deviceID)) return;
        PlayerIDs.Add(currentID);
        DeviceIDs.Add(deviceID);
        PlayerSelections_IMG[currentID].color = Color.white;
        currentID++;
    }
}