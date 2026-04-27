using UnityEngine;

public class SimpleEnemyAI : BaseHazard
{
    public enum TargetSearchMode
    {
        GlobalSlowSearch,
        RangeFastSearch
    }

    public enum PushMode
    {
        InstantImpulse,
        SmoothPush
    }

    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    [Header("Target Search")]
    public TargetSearchMode targetSearchMode = TargetSearchMode.RangeFastSearch;
    public string playerTag = "Player";
    public float retargetInterval = 0.25f;

    [Header("Search Range")]
    public float detectionRange = 12f;

    [Header("Movement")]
    public float globalSearchMoveSpeed = 2.5f;
    public float rangeSearchMoveSpeed = 4f;
    public float attackRange = 1.8f;
    public bool lockYMovement = true;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public PushMode pushMode = PushMode.SmoothPush;

    [Header("Instant Push")]
    public float pushForce = 8f;
    public float upwardForce = 1.5f;

    [Header("Smooth Push")]
    public float smoothPushDuration = 0.25f;
    public float smoothPushForce = 35f;
    public float smoothUpwardForce = 1f;
    public float maxPushSpeed = 10f;

    [Header("Reset")]
    public bool resetToStartPosition = true;

    private Transform target;
    private Rigidbody rb;
    private EnemyState currentState;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float lastAttackTime = -999f;
    private float retargetTimer;

    private Rigidbody pushedRb;
    private Vector3 smoothPushDirection;
    private float smoothPushTimer;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        ResetHazard();
    }

    public override void UpdateHazard()
    {
        UpdateSmoothPush();

        UpdateTarget();

        if (target == null)
        {
            currentState = EnemyState.Idle;
            StopMoving();
            return;
        }

        UpdateState();
        UpdateBehavior();
    }

    public override void ResetHazard()
    {
        currentState = EnemyState.Idle;
        lastAttackTime = -999f;
        retargetTimer = 0f;
        target = null;

        pushedRb = null;
        smoothPushDirection = Vector3.zero;
        smoothPushTimer = 0f;

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

    private void UpdateTarget()
    {
        retargetTimer -= Time.deltaTime;

        if (retargetTimer > 0f && target != null)
        {
            if (targetSearchMode == TargetSearchMode.RangeFastSearch && !IsTargetInDetectionRange(target))
            {
                target = null;
            }

            return;
        }

        retargetTimer = retargetInterval;

        if (targetSearchMode == TargetSearchMode.GlobalSlowSearch)
        {
            target = FindClosestPlayerGlobal();
        }
        else
        {
            target = FindClosestPlayerInRange();
        }
    }

    private Transform FindClosestPlayerGlobal()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        Transform closestPlayer = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (GameObject player in players)
        {
            Vector3 offset = player.transform.position - transform.position;

            if (lockYMovement)
            {
                offset.y = 0f;
            }

            float distanceSqr = offset.sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestPlayer = player.transform;
            }
        }

        return closestPlayer;
    }

    private Transform FindClosestPlayerInRange()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        Transform closestPlayer = null;
        float closestDistanceSqr = detectionRange * detectionRange;

        foreach (GameObject player in players)
        {
            Vector3 offset = player.transform.position - transform.position;

            if (lockYMovement)
            {
                offset.y = 0f;
            }

            float distanceSqr = offset.sqrMagnitude;

            if (distanceSqr <= closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestPlayer = player.transform;
            }
        }

        return closestPlayer;
    }

    private bool IsTargetInDetectionRange(Transform possibleTarget)
    {
        if (possibleTarget == null)
        {
            return false;
        }

        Vector3 offset = possibleTarget.position - transform.position;

        if (lockYMovement)
        {
            offset.y = 0f;
        }

        return offset.sqrMagnitude <= detectionRange * detectionRange;
    }

    private void UpdateState()
    {
        if (target == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distanceToTarget = GetHorizontalDistanceToTarget();

        if (distanceToTarget <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else
        {
            currentState = EnemyState.Chasing;
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
        if (rb == null || target == null)
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

        float currentMoveSpeed = GetCurrentMoveSpeed();
        Vector3 velocity = direction * currentMoveSpeed;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );

        transform.forward = direction;
    }

    private float GetCurrentMoveSpeed()
    {
        if (targetSearchMode == TargetSearchMode.GlobalSlowSearch)
        {
            return globalSearchMoveSpeed;
        }

        return rangeSearchMoveSpeed;
    }

    private float GetHorizontalDistanceToTarget()
    {
        if (target == null)
        {
            return float.MaxValue;
        }

        Vector3 offset = target.position - transform.position;

        if (lockYMovement)
        {
            offset.y = 0f;
        }

        return offset.magnitude;
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
        if (target == null)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        Rigidbody targetRb = target.GetComponentInParent<Rigidbody>();

        if (targetRb != null)
        {
            if (pushMode == PushMode.InstantImpulse)
            {
                ApplyInstantPush(targetRb);
            }
            else
            {
                StartSmoothPush(targetRb);
            }
        }

        lastAttackTime = Time.time;
    }

    private void ApplyInstantPush(Rigidbody targetRb)
    {
        Vector3 direction = GetPushDirection(targetRb);

        Vector3 force = direction * pushForce;
        force += Vector3.up * upwardForce;

        targetRb.AddForce(force, ForceMode.Impulse);
    }

    private void StartSmoothPush(Rigidbody targetRb)
    {
        pushedRb = targetRb;
        smoothPushDirection = GetPushDirection(targetRb);
        smoothPushTimer = smoothPushDuration;
    }

    private void UpdateSmoothPush()
    {
        if (pushMode != PushMode.SmoothPush)
        {
            return;
        }

        if (pushedRb == null)
        {
            return;
        }

        if (smoothPushTimer <= 0f)
        {
            pushedRb = null;
            smoothPushDirection = Vector3.zero;
            return;
        }

        smoothPushTimer -= Time.deltaTime;

        Vector3 horizontalVelocity = new Vector3(
            pushedRb.linearVelocity.x,
            0f,
            pushedRb.linearVelocity.z
        );

        float speedInPushDirection = Vector3.Dot(horizontalVelocity, smoothPushDirection);

        if (speedInPushDirection < maxPushSpeed)
        {
            pushedRb.AddForce(smoothPushDirection * smoothPushForce, ForceMode.Acceleration);
        }

        if (smoothUpwardForce > 0f)
        {
            pushedRb.AddForce(Vector3.up * smoothUpwardForce, ForceMode.Acceleration);
        }
    }

    private Vector3 GetPushDirection(Rigidbody targetRb)
    {
        Vector3 direction = targetRb.transform.position - transform.position;

        if (lockYMovement)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        direction.Normalize();

        return direction;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetSearchMode == TargetSearchMode.RangeFastSearch)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}