using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    public int PlayerIndex = -1;
    [Header("Stats")]
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentStamina;
    public int MaxStamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartPlayer(bool canJump, int playerIndex)
    {
        PlayerMovement.StartMovement(canJump);
        PlayerIndex = playerIndex;
    }

    // Update is called once per frame
    public void UpdatePlayer()
    {
        PlayerMovement.UpdateMovement();
    }

    public void FixedUpdatePlayer()
    {
        PlayerMovement.UpdateFixedMovement();
    }
}
