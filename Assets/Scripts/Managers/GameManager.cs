using UnityEditor.Purchasing;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public HazardManager HazardManager;
    public GameHUDManager GameHUDManager;

    public int Minutes;
    private int startMinutes;
    public float Seconds;
    private float startSeconds;
    private bool gameIsRunning = true;
    public GameObject SplashScreen;
    [Header("Debug")]
    public bool enableDebugReset = true;

    protected virtual void Awake()
    {
        startMinutes = Minutes;
        startSeconds = Seconds;
        HazardManager.WakeUpManager();
    }

    protected virtual void Start()
    {
        PlayerManager.StartManager(true);
        HazardManager.StartManager();
        GameHUDManager.StartHUD(PlayerManager.ActivePlayers);
    }

    protected virtual void Update()
    {
        if (SplashScreen) Destroy(SplashScreen);
        if (gameIsRunning == false) return;

        PlayerManager.UpdateManager();
        HazardManager.UpdateManager();

        if (enableDebugReset && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            HazardManager.ResetHazards();
            Debug.Log("Hazards reset.");
        }

        Seconds -= Time.deltaTime;
        if (Seconds < 0)
        {
            Minutes--;
            Seconds = 60;
        }
        if (Minutes < 0)
        {
            gameIsRunning = false;
            GameOver();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (gameIsRunning == false) return;
        PlayerManager.FixedUpdateManager();
        HazardManager.FixedUpdateManager();
    }

    protected virtual void LateUpdate()
    {
        //Update UI here if needed
        GameHUDManager.UpdateHUD(Minutes, Seconds);
    }

    protected virtual void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        gameIsRunning = false;
        GameHUDManager.GameOver();
        yield return new WaitForSeconds(0.3f);
        LevelManager.Instance.LoadNextScene();
    }


    protected virtual void ResetGame()
    {
        //PlayerManager.ResetManager();
        //HazardManager.ResetManager();
        Minutes = startMinutes;
        Seconds = startSeconds;
    }
}