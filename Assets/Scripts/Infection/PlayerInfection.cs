using UnityEngine;

public class PlayerInfection : MonoBehaviour
{
    public enum InfectionState
    {
        Human,
        Infected,
        Winner
    }

    [Header("State")]
    public InfectionState currentState = InfectionState.Human;

    [Header("Infection Rules")]
    public float infectCooldownAfterTurning = 1f;
    public bool canInfectImmediately = false;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private float infectCooldownTimer;
    private InfectionGameManager infectionGameManager;

    public bool IsHuman
    {
        get { return currentState == InfectionState.Human; }
    }

    public bool IsInfected
    {
        get { return currentState == InfectionState.Infected; }
    }

    public bool IsWinner
    {
        get { return currentState == InfectionState.Winner; }
    }

    public bool CanInfect
    {
        get { return IsInfected && infectCooldownTimer <= 0f; }
    }

    private void Awake()
    {
        infectionGameManager = FindFirstObjectByType<InfectionGameManager>();
    }

    private void Start()
    {
        if (infectionGameManager != null)
        {
            infectionGameManager.RegisterPlayer(this);
        }
    }

    private void Update()
    {
        if (infectCooldownTimer > 0f)
        {
            infectCooldownTimer -= Time.deltaTime;
        }
    }

    public void SetHuman()
    {
        currentState = InfectionState.Human;
        infectCooldownTimer = 0f;

        if (showDebugLogs)
        {
            Debug.Log(name + " set to Human.");
        }
    }

    public void Infect()
    {
        if (!IsHuman)
        {
            return;
        }

        currentState = InfectionState.Infected;

        if (canInfectImmediately)
        {
            infectCooldownTimer = 0f;
        }
        else
        {
            infectCooldownTimer = infectCooldownAfterTurning;
        }

        if (showDebugLogs)
        {
            Debug.Log(name + " became Infected.");
        }

        if (infectionGameManager != null)
        {
            infectionGameManager.OnPlayerInfected(this);
        }
    }

    public void SetWinner()
    {
        currentState = InfectionState.Winner;
        infectCooldownTimer = 0f;

        if (showDebugLogs)
        {
            Debug.Log(name + " is the Winner.");
        }
    }

    public void SetManager(InfectionGameManager manager)
    {
        infectionGameManager = manager;
    }
}