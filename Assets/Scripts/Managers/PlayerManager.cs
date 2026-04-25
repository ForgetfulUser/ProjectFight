using NUnit.Framework.Internal.Commands;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Player Player_PRFB; // If not using the spawner, assign the player to this
    public List<Transform> PlayerSpawnPosition_TRSFMs = new List<Transform>();
    public List<Player> ActivePlayers = new List<Player>();

    [Header("Play Test Variables")]
    public bool ShouldSpawnPlayers = true;

    public void StartManager(bool canJump)
    {
        SpawnPlayers(canJump);
        Player_PRFB.StartPlayer(canJump, 0);
    }

    public void UpdateManager()
    {
        if (ShouldSpawnPlayers)
        {
            foreach(var player in ActivePlayers)
            {
                player.UpdatePlayer();
            }
        }
        else
        {
            Player_PRFB.UpdatePlayer();
        }

    }

    private void SpawnPlayers(bool canJump)
    {
        if (!ShouldSpawnPlayers) return;

        for (int i = 0; i < PlayerSpawnPosition_TRSFMs.Count; i++)
        {
            Player player = Instantiate(Player_PRFB, PlayerSpawnPosition_TRSFMs[i].position, Quaternion.identity, transform);
            player.StartPlayer(canJump, i);
            ActivePlayers.Add(player);
        }
    }

    public void RespawnPlayer(Player player)
    {
        player.transform.position = PlayerSpawnPosition_TRSFMs[player.PlayerIndex].position;
    }
}
