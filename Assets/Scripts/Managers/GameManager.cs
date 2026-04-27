using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public HazardManager HazardManager;

    [Header("Debug")]
    public bool enableDebugReset = true;

    protected virtual void Awake()
    {
        HazardManager.WakeUpManager();
    }

    protected virtual void Start()
    {
        PlayerManager.StartManager(true);
        HazardManager.StartManager();
    }

    protected virtual void Update()
    {
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
}