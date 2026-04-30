using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public HazardManager HazardManager;
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
    }

    protected virtual void FixedUpdate()
    {
        PlayerManager.FixedUpdateManager();
        HazardManager.FixedUpdateManager();
    }

    protected virtual void LateUpdate()
    {
        //Update UI here if needed
    }

    protected virtual void ResetGame()
    {
        //PlayerManager.ResetManager();
        //HazardManager.ResetManager();
        Minutes = startMinutes;
        Seconds = startSeconds;
    }
}