using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        LevelManager.Instance.LoadNextScene();
    }
}
