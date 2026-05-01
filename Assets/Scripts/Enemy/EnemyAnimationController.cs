using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public NavMeshAgent agent;

    [Header("Animator Parameters")]
    public string isMovingParameter = "IsMoving";
    public string pushTriggerParameter = "Push";

    [Header("Movement Check")]
    public float movingSpeedThreshold = 0.05f;

    private bool forceMovingAnimation;
    private bool isAttacking;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null)
        {
            return;
        }

        bool isMoving = false;

        if (!isAttacking)
        {
            isMoving = forceMovingAnimation;

            if (!isMoving && agent != null && agent.enabled && agent.isOnNavMesh)
            {
                Vector3 velocity = agent.velocity;
                velocity.y = 0f;

                isMoving = velocity.sqrMagnitude > movingSpeedThreshold * movingSpeedThreshold;
            }
        }

        animator.SetBool(isMovingParameter, isMoving);
    }

    public void SetForceMovingAnimation(bool forceMoving)
    {
        forceMovingAnimation = forceMoving;
    }

    public void PlayPush()
    {
        if (animator == null)
        {
            return;
        }

        isAttacking = true;
        forceMovingAnimation = false;

        animator.SetBool(isMovingParameter, false);

        animator.ResetTrigger(pushTriggerParameter);
        animator.SetTrigger(pushTriggerParameter);
    }

    public void EndAttackAnimationLock()
    {
        isAttacking = false;
    }
}