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
    private float preGameTimer = 3f;
    private float preGameTimerMulti = 1.2f;
    private float startSeconds;
    private bool isGameRunning = true;
    private bool isPreGame = true;
    public GameObject SplashScreen;
    [Header("Debug")]
    public bool enableDebugReset = true;
    public bool skipPreGame = false;

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

        if (skipPreGame)
        {
            preGameTimer = 0f;
        }
    }

    protected virtual void UpdateServer()
    {
        // if(IsServer == false) return;
        /// Packets to Recieve
        /// - player input
        /// Server relevant functions
        /// - player movement
        /// - enemy movement
        /// - collision triggers
    }

    protected virtual void Update()
    {
        if (preGameTimer >= 0)
        {
            preGameTimer -= Time.deltaTime * preGameTimerMulti;

            if(preGameTimer <= 1f)
            {
                isPreGame = false;
            }
            GameHUDManager.UpdatePreGameTimer(preGameTimer);
        }

        if (SplashScreen) Destroy(SplashScreen);
        if (isGameRunning == false) return;

        PlayerManager.UpdateManager();
        HazardManager.UpdateManager();

        if (enableDebugReset && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            HazardManager.ResetHazards();
            Debug.Log("Hazards reset.");
        }

        if (WinCheck() != null)
        {
            Seconds = 0;
            Minutes = 0;
        }

        Seconds -= Time.deltaTime;
        if (Seconds < 0)
        {
            Minutes--;
            Seconds = 60;
        }
        if (Minutes < 0)
        {
            isGameRunning = false;
            GameOver();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isGameRunning == false || isPreGame == true) return;
        PlayerManager.FixedUpdateManager();
        HazardManager.FixedUpdateManager();
    }

    protected virtual void LateUpdate()
    {
        GameHUDManager.UpdateHUD(Minutes, Seconds);
    }

    protected virtual void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        isGameRunning = false;
        int finalPlayerIndex = PlayerManager.lastPlayerStanding ? PlayerManager.lastPlayerStanding.PlayerIndex : -1;
        GameHUDManager.GameOver(finalPlayerIndex);
        yield return new WaitForSeconds(0.7f);
        LevelManager.Instance.LoadNextScene();
    }


    protected virtual void ResetGame()
    {
        //PlayerManager.ResetManager();
        //HazardManager.ResetManager();
        Minutes = startMinutes;
        Seconds = startSeconds;
    }

    protected virtual Player WinCheck()
    {
        return PlayerManager.LastPlayerStanding();
    }
}