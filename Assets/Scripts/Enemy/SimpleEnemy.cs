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
        Attacking,
        Disabled
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
    public float instantPushVelocity = 8f;
    public float instantUpwardVelocity = 3f;
    public float instantStunTime = 0.2f;

    [Header("Smooth Push")]
    public float smoothPushDuration = 0.25f;
    public float smoothPushForce = 35f;
    public float smoothUpwardVelocity = 3f;
    public float smoothStunTime = 0.12f;
    public float maxPushSpeed = 10f;

    [Header("Void / Out Of Bounds")]
    public bool resetWhenBelowVoid = true;
    public bool disableWhenBelowVoid = false;
    public float voidY = -10f;

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
    private PlayerMovement pushedPlayerMovement;
    private Vector3 smoothPushDirection;
    private float smoothPushTimer;
    private bool smoothUpwardApplied;

    private bool hasStarted;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        hasStarted = true;

        ResetHazard();
    }

    public override void UpdateHazard()
    {
        if (!hasStarted)
        {
            return;
        }

        if (CheckVoidFall())
        {
            return;
        }

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

        ClearSmoothPush();

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

        if (gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);
        }
    }

    private bool CheckVoidFall()
    {
        if (transform.position.y >= voidY)
        {
            return false;
        }

        target = null;
        currentState = EnemyState.Disabled;
        ClearSmoothPush();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (disableWhenBelowVoid)
        {
            gameObject.SetActive(false);
            return true;
        }

        if (resetWhenBelowVoid)
        {
            ResetHazard();
            return true;
        }

        return true;
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
            if (player == null)
            {
                continue;
            }

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
            if (player == null)
            {
                continue;
            }

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
        if (currentState == EnemyState.Disabled)
        {
            StopMoving();
            return;
        }

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

        if (transform.position.y < voidY)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        Rigidbody targetRb = target.GetComponentInParent<Rigidbody>();
        PlayerMovement targetPlayerMovement = target.GetComponentInParent<PlayerMovement>();

        if (targetRb != null && targetPlayerMovement != null)
        {
            if (pushMode == PushMode.InstantImpulse)
            {
                ApplyInstantPush(targetRb, targetPlayerMovement);
            }
            else
            {
                StartSmoothPush(targetRb, targetPlayerMovement);
            }
        }

        lastAttackTime = Time.time;
    }

    private void ApplyInstantPush(Rigidbody targetRb, PlayerMovement targetPlayerMovement)
    {
        Vector3 direction = GetPushDirection(targetRb);

        Vector3 velocityChange = direction * instantPushVelocity;

        targetPlayerMovement.AddExternalVelocity(velocityChange);

        if (instantUpwardVelocity > 0f)
        {
            targetPlayerMovement.AddVerticalVelocity(instantUpwardVelocity);
        }

        if (instantStunTime > 0f)
        {
            targetPlayerMovement.Stun(instantStunTime);
        }
    }

    private void StartSmoothPush(Rigidbody targetRb, PlayerMovement targetPlayerMovement)
    {
        pushedRb = targetRb;
        pushedPlayerMovement = targetPlayerMovement;
        smoothPushDirection = GetPushDirection(targetRb);
        smoothPushTimer = smoothPushDuration;
        smoothUpwardApplied = false;

        ApplySmoothUpwardOnce();
    }

    private void UpdateSmoothPush()
    {
        if (pushMode != PushMode.SmoothPush)
        {
            return;
        }

        if (pushedRb == null || pushedPlayerMovement == null)
        {
            return;
        }

        if (transform.position.y < voidY)
        {
            ClearSmoothPush();
            return;
        }

        if (smoothPushTimer <= 0f)
        {
            ClearSmoothPush();
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
            Vector3 velocityChange =
                smoothPushDirection * smoothPushForce * Time.deltaTime;

            pushedPlayerMovement.AddExternalVelocity(velocityChange);
        }

        if (smoothStunTime > 0f)
        {
            pushedPlayerMovement.Stun(smoothStunTime);
        }
    }

    private void ApplySmoothUpwardOnce()
    {
        if (smoothUpwardApplied)
        {
            return;
        }

        if (pushedPlayerMovement == null)
        {
            return;
        }

        if (smoothUpwardVelocity <= 0f)
        {
            return;
        }

        pushedPlayerMovement.AddVerticalVelocity(smoothUpwardVelocity);
        smoothUpwardApplied = true;
    }

    private void ClearSmoothPush()
    {
        pushedRb = null;
        pushedPlayerMovement = null;
        smoothPushDirection = Vector3.zero;
        smoothPushTimer = 0f;
        smoothUpwardApplied = false;
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

        Gizmos.color = Color.magenta;
        Vector3 voidLineCenter = new Vector3(transform.position.x, voidY, transform.position.z);
        Gizmos.DrawWireCube(voidLineCenter, new Vector3(2f, 0.05f, 2f));
    }
}