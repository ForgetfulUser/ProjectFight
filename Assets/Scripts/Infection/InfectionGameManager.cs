using System.Collections.Generic;
using UnityEngine;

public class InfectionGameManager : MonoBehaviour
{
    [Header("Players")]
    public List<PlayerInfection> players = new List<PlayerInfection>();

    [Header("Round State")]
    public bool roundStarted = false;
    public bool roundActive = false;
    public PlayerInfection winner;

    [Header("Start Rules")]
    public int minPlayersToStart = 2;
    public bool autoStartWhenEnoughPlayers = true;
    public float autoStartDelay = 0.5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private bool waitingForAutoStart;
    private float autoStartTimer;

    private void Update()
    {
        UpdateAutoStart();
    }

    private void UpdateAutoStart()
    {
        if (!autoStartWhenEnoughPlayers)
        {
            return;
        }

        if (!waitingForAutoStart)
        {
            return;
        }

        autoStartTimer -= Time.deltaTime;

        if (autoStartTimer <= 0f)
        {
            waitingForAutoStart = false;
            StartRound();
        }
    }

    public void RegisterPlayer(PlayerInfection player)
    {
        if (player == null)
        {
            return;
        }

        if (!players.Contains(player))
        {
            players.Add(player);
            player.SetManager(this);
        }

        if (showDebugLogs)
        {
            Debug.Log("Registered infection player: " + player.name + " | Total: " + players.Count);
        }

        if (!roundStarted && autoStartWhenEnoughPlayers && players.Count >= minPlayersToStart)
        {
            waitingForAutoStart = true;
            autoStartTimer = autoStartDelay;
        }
    }

    public void UnregisterPlayer(PlayerInfection player)
    {
        if (player == null)
        {
            return;
        }

        if (players.Contains(player))
        {
            players.Remove(player);
        }
    }

    public void StartRound()
    {
        if (roundStarted)
        {
            return;
        }

        if (players.Count < minPlayersToStart)
        {
            if (showDebugLogs)
            {
                Debug.Log("Not enough players to start infection round.");
            }

            return;
        }

        roundStarted = true;
        roundActive = true;
        waitingForAutoStart = false;
        winner = null;

        if (showDebugLogs)
        {
            Debug.Log("Infection round started. Player count: " + players.Count);
        }

        CheckWinner();
    }

    public void OnPlayerInfected(PlayerInfection infectedPlayer)
    {
        if (!roundStarted || !roundActive)
        {
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log(infectedPlayer.name + " was infected.");
        }

        CheckWinner();
    }

    public void CheckWinner()
    {
        if (!roundStarted || !roundActive)
        {
            return;
        }

        PlayerInfection lastHuman = null;
        int humanCount = 0;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerInfection player = players[i];

            if (player == null)
            {
                continue;
            }

            if (player.IsHuman)
            {
                humanCount++;
                lastHuman = player;
            }
        }

        if (showDebugLogs)
        {
            Debug.Log("Human count: " + humanCount);
        }

        if (humanCount == 1 && lastHuman != null)
        {
            DeclareWinner(lastHuman);
        }
    }

    private void DeclareWinner(PlayerInfection player)
    {
        if (!roundActive)
        {
            return;
        }

        roundActive = false;
        winner = player;

        player.SetWinner();

        if (showDebugLogs)
        {
            Debug.Log("Winner: " + player.name);
        }
    }

    public void ResetRound()
    {
        roundStarted = false;
        roundActive = false;
        waitingForAutoStart = false;
        autoStartTimer = 0f;
        winner = null;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                players[i].SetHuman();
            }
        }

        if (showDebugLogs)
        {
            Debug.Log("Infection round reset.");
        }
    }

    public PlayerInfection GetClosestHuman(Vector3 position)
    {
        PlayerInfection closestHuman = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerInfection player = players[i];

            if (player == null)
            {
                continue;
            }

            if (!player.IsHuman)
            {
                continue;
            }

            float distanceSqr = (player.transform.position - position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestHuman = player;
            }
        }

        return closestHuman;
    }
}