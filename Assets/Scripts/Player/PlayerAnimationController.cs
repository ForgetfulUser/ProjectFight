using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator Animator;
    
    public void InitializeAnimation(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                Animator.SetBool("Is Player One", true);
                break;
            case 1:
                Animator.SetBool("Is Player Two", true);
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }

    public void UpdateAnimation(Vector2 moveDir)
    {
        Animator.SetInteger("Horizontal Movement", (int)moveDir.x);
        Animator.SetInteger("Vertical Movement", (int)moveDir.y);
    }
}
