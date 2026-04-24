using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Player player; // Will be a list

    public void StartManager(bool canJump)
    {
        player.StartPlayer(canJump);
    }

    public void UpdateManager()
    {
        player.UpdatePlayer();
    }
}
