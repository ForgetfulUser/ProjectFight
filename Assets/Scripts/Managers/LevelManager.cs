using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public List<int> PlayerIDs = new List<int>();
    public int sceneIndex;

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
        this.sceneIndex = sceneIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadNextScene()
    {
        sceneIndex++;
        if (sceneIndex >= SceneManager.sceneCountInBuildSettings) sceneIndex = 0;
        SceneManager.LoadScene(sceneIndex);
    }
}