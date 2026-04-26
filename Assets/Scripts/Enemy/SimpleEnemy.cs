using UnityEngine;

public class SimpleEnemyAI : BaseHazard
{
    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    [Header("Target")]
    public Transform target;
    public string playerTag = "Player";

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float detectionRange = 12f;
    public float attackRange = 1.8f;
    public bool lockYMovement = true;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public float pushForce = 8f;
    public float upwardForce = 1.5f;

    [Header("Reset")]
    public bool resetToStartPosition = true;

    private Rigidbody rb;
    private EnemyState currentState;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float lastAttackTime = -999f;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        FindTarget();

        ResetHazard();
    }

    public override void UpdateHazard()
    {
        if (target == null)
        {
            FindTarget();

            if (target == null)
            {
                StopMoving();
                return;
            }
        }

        UpdateState();
        UpdateBehavior();
    }

    public override void ResetHazard()
    {
        currentState = EnemyState.Idle;
        lastAttackTime = -999f;

        if (resetToStartPosition)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void FindTarget()
    {
        if (target != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void UpdateState()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distanceToTarget <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    private void UpdateBehavior()
    {
        if (currentState == EnemyState.Idle)
        {
            StopMoving();
        }
        else if (currentState == EnemyState.Chasing)
        {
            ChaseTarget();
        }
        else if (currentState == EnemyState.Attacking)
        {
            StopMoving();
            TryAttack();
        }
    }

    private void ChaseTarget()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;

        if (lockYMovement)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            StopMoving();
            return;
        }

        direction.Normalize();

        Vector3 velocity = direction * moveSpeed;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );

        transform.forward = direction;
    }

    private void StopMoving()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        Rigidbody targetRb = target.GetComponentInParent<Rigidbody>();

        if (targetRb != null)
        {
            Vector3 direction = target.position - transform.position;

            if (lockYMovement)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = transform.forward;
            }

            direction.Normalize();

            Vector3 force = direction * pushForce;
            force += Vector3.up * upwardForce;

            targetRb.AddForce(force, ForceMode.Impulse);
        }

        lastAttackTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}