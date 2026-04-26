using UnityEngine;

public class SimpleEnemyAI : MonoBehaviour
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
    public float stopDistance = 1.5f;

    [Header("Attack")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1f;
    public float knockbackForce = 8f;
    public float upwardForce = 1.5f;

    [Header("Ground Movement")]
    public bool lockYMovement = true;

    private Rigidbody rb;
    private EnemyState state;
    private float lastAttackTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        state = EnemyState.Idle;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null || rb == null)
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            state = EnemyState.Attacking;
        }
        else if (distanceToTarget <= detectionRange)
        {
            state = EnemyState.Chasing;
        }
        else
        {
            state = EnemyState.Idle;
        }

        if (state == EnemyState.Idle)
        {
            StopMoving();
        }
        else if (state == EnemyState.Chasing)
        {
            ChaseTarget();
        }
        else if (state == EnemyState.Attacking)
        {
            StopMoving();
            TryAttack();
        }
    }

    private void ChaseTarget()
    {
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

            Vector3 force = direction * knockbackForce;
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