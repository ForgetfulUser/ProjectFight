using NUnit.Framework.Internal.Commands;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Player Player_PRFB;
    private List<Vector3> PlayerSpawnPosition = new List<Vector3>();
    public List<Player> ActivePlayers = new List<Player>();
    public float MinimumFallHeight = -10;
    [Header("Play Test Variables")]
    public Player TestingPlayer;
    private List<int> PlayerIDs = new List<int>();

    public void StartManager(bool canJump)
    {
        foreach(var spawnPos in GetComponentsInChildren<Transform>())
        {
            if (spawnPos == transform) continue;
            PlayerSpawnPosition.Add(spawnPos.position);
            Destroy(spawnPos.gameObject);
        }
        PlayerIDs = LevelManager.Instance.PlayerIDs;
        SpawnPlayers(canJump);
        if (TestingPlayer) 
            Player_PRFB.StartPlayer(canJump, 0);
    }

    public void UpdateManager()
    {
        if (TestingPlayer == null)
        {
            foreach(var player in ActivePlayers)
            {
                if (player.IsActive)
                {
                    player.UpdatePlayer();
                    ResetTest(player);
                }
            }
        }
        else
        {
            TestingPlayer.UpdatePlayer();
            ResetTest(TestingPlayer);
        }
    }

    public void FixedUpdateManager()
    {
        if (TestingPlayer == null)
        {
            foreach (var player in ActivePlayers)
            {
                if(player.IsActive)
                    player.FixedUpdatePlayer();
            }
        }
        else
        {
            TestingPlayer.FixedUpdatePlayer();
        }
    }

    public void ResetTest(Player player)
    {
        if(player.transform.position.y <= MinimumFallHeight)
        {
            RespawnPlayer(player);
        }
    }

    private void SpawnPlayers(bool canJump)
    {
        if (TestingPlayer) return;

        for (int i = 0; i < LevelManager.Instance.PlayerIDs.Count; i++)
        {
            Player player = Instantiate(Player_PRFB, PlayerSpawnPosition[i], Quaternion.identity);
            player.StartPlayer(canJump, PlayerIDs[i]);
            ActivePlayers.Add(player);
        }
    }

    public void RespawnPlayer(Player player)
    {
        if (player.IsActive == false) return;
        
        player.CurrentLives--;
        if (player.CurrentLives < 0)
        {
            player.gameObject.SetActive(false);
            player.IsActive = false;
        }

        player.transform.position = PlayerSpawnPosition[player.PlayerIndex];
    }
}
