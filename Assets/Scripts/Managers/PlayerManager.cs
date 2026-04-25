using NUnit.Framework.Internal.Commands;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Player Player_PRFB;
    private List<Vector3> PlayerSpawnPosition = new List<Vector3>();
    public List<Player> ActivePlayers = new List<Player>();

    [Header("Play Test Variables")]
    public Player TestingPlayer;

    public void StartManager(bool canJump)
    {
        foreach(var spawnPos in GetComponentsInChildren<Transform>())
        {
            if (spawnPos == transform) continue;
            PlayerSpawnPosition.Add(spawnPos.position);
            Destroy(spawnPos.gameObject);
        }
        SpawnPlayers(canJump);
        Player_PRFB.StartPlayer(canJump, 0);
    }

    public void UpdateManager()
    {
        if (TestingPlayer == null)
        {
            foreach(var player in ActivePlayers)
            {
                player.UpdatePlayer();
            }
        }
        else
        {
            TestingPlayer.UpdatePlayer();
        }

    }

    private void SpawnPlayers(bool canJump)
    {
        if (TestingPlayer) return;

        for (int i = 0; i < PlayerSpawnPosition.Count; i++)
        {
            Player player = Instantiate(Player_PRFB, PlayerSpawnPosition[i], Quaternion.identity);
            player.StartPlayer(canJump, i);
            ActivePlayers.Add(player);
        }
    }

    public void RespawnPlayer(Player player)
    {
        player.transform.position = PlayerSpawnPosition[player.PlayerIndex];
    }
}
