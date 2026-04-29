using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("NavMesh")]
    public NavMeshAgent agent;
    public float stoppingDistance = 1.5f;

    [Header("NavMesh Link Jump")]
    public bool useCustomLinkJump = true;
    public float linkJumpHeight = 2.5f;
    public float linkJumpDuration = 0.45f;

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

    [Header("Debug")]
    public bool drawGizmos = true;

    private Transform target;
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
    private bool isTraversingLink;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        startPosition = transform.position;
        startRotation = transform.rotation;

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.autoTraverseOffMeshLink = false;
        }

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

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (useCustomLinkJump && agent.isOnOffMeshLink && !isTraversingLink)
        {
            StartCoroutine(TraverseLinkJump());
            return;
        }

        if (isTraversingLink)
        {
            return;
        }

        UpdateTarget();

        if (target == null)
        {
            currentState = EnemyState.Idle;
            StopAgent();
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
        isTraversingLink = false;

        ClearSmoothPush();

        if (gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (resetToStartPosition)
        {
            if (agent != null && agent.enabled)
            {
                agent.Warp(startPosition);
            }
            else
            {
                transform.position = startPosition;
            }

            transform.rotation = startRotation;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.stoppingDistance = stoppingDistance;
            agent.autoTraverseOffMeshLink = false;
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

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
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
            offset.y = 0f;

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
        offset.y = 0f;

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
            StopAgent();
            return;
        }

        if (currentState == EnemyState.Idle)
        {
            StopAgent();
        }
        else if (currentState == EnemyState.Chasing)
        {
            ChaseTarget();
        }
        else if (currentState == EnemyState.Attacking)
        {
            StopAgent();
            TryAttack();
        }
    }

    private void ChaseTarget()
    {
        if (agent == null || target == null)
        {
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;
        agent.speed = GetCurrentMoveSpeed();
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);
    }

    private void StopAgent()
    {
        if (agent == null)
        {
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    private IEnumerator TraverseLinkJump()
    {
        if (agent == null)
        {
            yield break;
        }

        isTraversingLink = true;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        Vector3 startPos = transform.position;
        Vector3 endPos = linkData.endPos;

        float timer = 0f;

        agent.isStopped = true;
        agent.updatePosition = false;

        while (timer < linkJumpDuration)
        {
            float t = timer / linkJumpDuration;

            Vector3 flatPosition = Vector3.Lerp(startPos, endPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * linkJumpHeight;

            transform.position = new Vector3(
                flatPosition.x,
                flatPosition.y + arc,
                flatPosition.z
            );

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        agent.updatePosition = true;
        agent.Warp(endPos);
        agent.CompleteOffMeshLink();

        agent.isStopped = false;
        isTraversingLink = false;
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
        offset.y = 0f;

        return offset.magnitude;
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
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector3.forward;
        }

        direction.Normalize();

        return direction;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
        {
            return;
        }

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