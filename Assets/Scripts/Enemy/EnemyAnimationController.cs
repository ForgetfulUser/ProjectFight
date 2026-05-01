using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Animator Parameters")]
    public string pushTriggerParameter = "Push";
    public string jumpTriggerParameter = "Jump";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void PlayPush()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(pushTriggerParameter);
        animator.SetTrigger(pushTriggerParameter);
    }

    public void PlayJump()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(jumpTriggerParameter);
        animator.SetTrigger(jumpTriggerParameter);
    }
}