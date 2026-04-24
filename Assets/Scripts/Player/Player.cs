using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement PlayerMovement;

    [Header("Stats")]
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentStamina;
    public int MaxStamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartPlayer(bool canJump)
    {
        PlayerMovement.StartMovement(canJump);
    }

    // Update is called once per frame
    public void UpdatePlayer()
    {
        PlayerMovement.UpdateMovement();
    }
}
