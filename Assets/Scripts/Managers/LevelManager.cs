using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    //public List<int> PlayerIDs = new List<int>();
    public Dictionary<int, int> playerIDs; // Device | Player ID
    public int sceneIndex;
    bool isLoading = false;

    void OnEnable()
    {
        // Subscribe to the event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(int sceneIndex)
    {
        if (isLoading) return;
        isLoading = true;
        this.sceneIndex = sceneIndex;
        SceneManager.LoadScene(sceneIndex);
    }


    public void LoadNextScene()
    {
        if (isLoading) return;
        isLoading = true;
        sceneIndex++;
        if (sceneIndex >= SceneManager.sceneCountInBuildSettings) sceneIndex = 0;
        SceneManager.LoadScene(sceneIndex);
    }
}