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

    [Header("Infection")]
    public bool infectPlayerOnAttack = true;

    [Header("Void / Out Of Bounds")]
    public bool resetWhenBelowVoid = true;
    public bool disableWhenBelowVoid = false;
    public float voidY = -10f;

    [Header("Reset")]
    public bool resetToStartPosition = true;

    [Header("Animation")]
    public EnemyAnimationController enemyAnimationController;

    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool drawGizmos = true;

    private Transform target;
    private EnemyState currentState;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float lastAttackTime = -999f;
    private float retargetTimer;

    private bool hasStarted;
    private bool isTraversingLink;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        if (enemyAnimationController == null)
        {
            enemyAnimationController = GetComponent<EnemyAnimationController>();
        }

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
            if (!IsValidHumanTarget(target))
            {
                target = null;
                return;
            }

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

            if (!IsValidHumanTarget(player.transform))
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

            if (!IsValidHumanTarget(player.transform))
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

    private bool IsValidHumanTarget(Transform possibleTarget)
    {
        if (possibleTarget == null)
        {
            return false;
        }

        PlayerInfection playerInfection = possibleTarget.GetComponentInParent<PlayerInfection>();

        if (playerInfection == null)
        {
            return false;
        }

        return playerInfection.IsHuman;
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

        if (enemyAnimationController != null)
        {
            enemyAnimationController.PlayJump();
        }

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

        if (Time.time < lastAttackTime + forceCooldown)
        {
            return;
        }

        PlayerMovement targetPlayerMovement = target.GetComponentInParent<PlayerMovement>();

        if (targetPlayerMovement == null)
        {
            return;
        }

        PlayerInfection targetInfection = target.GetComponentInParent<PlayerInfection>();

        if (targetInfection == null || !targetInfection.IsHuman)
        {
            target = null;
            return;
        }

        if (enemyAnimationController != null)
        {
            enemyAnimationController.PlayPush();
        }

        if (infectPlayerOnAttack)
        {
            targetInfection.Infect();

            if (showDebugLogs)
            {
                Debug.Log("Enemy infected player: " + targetInfection.name);
            }
        }

        if (doesApplyForce)
        {
            Vector3 force = BuildForceToPlayer(targetPlayerMovement);
            HitPlayer(targetPlayerMovement, force);

            if (showDebugLogs)
            {
                Debug.Log("Enemy hit player with force: " + force);
            }
        }

        lastAttackTime = Time.time;
        target = null;
    }

    private Vector3 BuildForceToPlayer(PlayerMovement playerMovement)
    {
        Vector3 direction = playerMovement.transform.position - transform.position;
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

        float clampedAngle = Mathf.Clamp01(angleOfForce);

        Vector3 horizontalForce = direction * forceAmount;
        Vector3 upwardForce = Vector3.up * forceAmount * clampedAngle * upwardForceMultiplier;

        return horizontalForce + upwardForce;
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