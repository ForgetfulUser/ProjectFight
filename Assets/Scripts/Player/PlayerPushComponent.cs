using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class PlayerPushComponent : MonoBehaviour
{
    [Header("Push Detection")]
    public Vector3 PushBoxSize = new Vector3(1f, 0.75f, 1f);
    public float pushBoxForwardOffset = 1f;
    public LayerMask PlayerLayer;
    private bool tryPush;

    [Header("Push Force")]
    public float pushForce = 55f;
    public float upwardVelocity = 3f;
    public float maxPushSpeed = 26f;
    private Vector3 pushDirection;

    [Header("After-Hit Coroutine Push")]
    public float pushDuration = 0.45f;
    public bool fadePushOverTime = true;
    public float stunTime = 0.18f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private readonly Dictionary<PlayerMovement, Whipper.PushInfo> activePushes =
        new Dictionary<PlayerMovement, Whipper.PushInfo>();

    private PlayerMovement ownerPlayerMovement;

    private void Awake()
    {
        ownerPlayerMovement = GetComponentInParent<PlayerMovement>();
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (showDebugLogs)
        {
            Debug.Log("Attack");
        }

        tryPush = context.performed;

        if (tryPush == false)
        {
            pushDirection = Vector3.zero;
        }
    }

    public void UpdatePushComponent(Vector2 moveDir)
    {
        if (tryPush == false && activePushes.Count == 0)
        {
            return;
        }

        if (tryPush == false)
        {
            return;
        }

        tryPush = false;

        if (showDebugLogs)
        {
            Debug.Log("Try Push");
        }

        pushDirection = GetPushDirection(moveDir);

        Vector3 boxPosition = transform.position + pushDirection * pushBoxForwardOffset;
        Quaternion boxRotation = Quaternion.LookRotation(pushDirection, Vector3.up);

        Collider[] hits = Physics.OverlapBox(
            boxPosition,
            PushBoxSize,
            boxRotation,
            PlayerLayer
        );

        if (hits.Length > 0 && showDebugLogs)
        {
            Debug.Log(hits.Length + " hits foudn");
        }

        foreach (Collider hit in hits)
        {
            RegisterHit(hit);
        }
    }

    private Vector3 GetPushDirection(Vector2 moveDir)
    {
        Vector3 direction = new Vector3(
            moveDir.x,
            0f,
            moveDir.y
        );

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector3.forward;
        }

        direction.Normalize();
        return direction;
    }

    private void RegisterHit(Collider hit)
    {
        if (showDebugLogs)
        {
            Debug.Log("Registering Hit: " + hit.name);
        }

        PlayerMovement playerMovement = hit.GetComponentInParent<PlayerMovement>();

        if (playerMovement == null)
        {
            return;
        }

        if (playerMovement == ownerPlayerMovement)
        {
            return;
        }

        Rigidbody targetRb = playerMovement.GetComponent<Rigidbody>();

        if (targetRb == null)
        {
            targetRb = playerMovement.GetComponentInParent<Rigidbody>();
        }

        if (targetRb == null)
        {
            return;
        }

        bool colliderLayerMatches = IsInLayerMask(hit.gameObject.layer, PlayerLayer);
        bool playerLayerMatches = IsInLayerMask(playerMovement.gameObject.layer, PlayerLayer);

        if (!colliderLayerMatches && !playerLayerMatches)
        {
            return;
        }

        if (activePushes.TryGetValue(playerMovement, out Whipper.PushInfo existingPush))
        {
            existingPush.rb = targetRb;
            existingPush.playerMovement = playerMovement;
            existingPush.pushDirection = pushDirection;
            existingPush.timer = pushDuration;

            return;
        }

        if (showDebugLogs)
        {
            Debug.Log("Applying Push");
        }

        Whipper.PushInfo pushInfo = new Whipper.PushInfo();
        pushInfo.rb = targetRb;
        pushInfo.playerMovement = playerMovement;
        pushInfo.pushDirection = pushDirection;
        pushInfo.timer = pushDuration;
        pushInfo.upwardApplied = false;
        pushInfo.coroutine = StartCoroutine(PushPlayerCoroutine(pushInfo));

        activePushes.Add(playerMovement, pushInfo);

        ApplyUpwardLaunch(pushInfo);
    }

    private IEnumerator PushPlayerCoroutine(Whipper.PushInfo pushInfo)
    {
        while (pushInfo != null && pushInfo.timer > 0f)
        {
            if (pushInfo.playerMovement == null || pushInfo.rb == null)
            {
                break;
            }

            float fade = 1f;

            if (fadePushOverTime && pushDuration > 0.001f)
            {
                fade = Mathf.Clamp01(pushInfo.timer / pushDuration);
            }

            ApplyPushVelocity(pushInfo, fade);

            pushInfo.timer -= Time.deltaTime;

            yield return null;
        }

        if (pushInfo != null && pushInfo.playerMovement != null)
        {
            if (activePushes.ContainsKey(pushInfo.playerMovement))
            {
                activePushes.Remove(pushInfo.playerMovement);
            }
        }
    }

    private void ApplyPushVelocity(Whipper.PushInfo pushInfo, float forceMultiplier)
    {
        if (pushInfo == null || pushInfo.playerMovement == null || pushInfo.rb == null)
        {
            return;
        }

        Vector3 currentPushDirection = pushInfo.pushDirection;

        if (currentPushDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        currentPushDirection.y = 0f;

        if (currentPushDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        currentPushDirection.Normalize();

        Vector3 horizontalVelocity = new Vector3(
            pushInfo.rb.linearVelocity.x,
            0f,
            pushInfo.rb.linearVelocity.z
        );

        float speedInPushDirection = Vector3.Dot(horizontalVelocity, currentPushDirection);

        if (speedInPushDirection < maxPushSpeed)
        {
            Vector3 velocityChange =
                currentPushDirection * pushForce * forceMultiplier * Time.deltaTime;

            pushInfo.playerMovement.AddExternalVelocity(velocityChange);
        }

        if (stunTime > 0f)
        {
            pushInfo.playerMovement.Stun(stunTime);
        }

        if (showDebugLogs)
        {
            Debug.Log("Comle");
        }
    }

    private void ApplyUpwardLaunch(Whipper.PushInfo pushInfo)
    {
        if (pushInfo == null || pushInfo.playerMovement == null)
        {
            return;
        }

        if (upwardVelocity <= 0f)
        {
            return;
        }

        if (pushInfo.upwardApplied)
        {
            return;
        }

        pushInfo.playerMovement.AddVerticalVelocity(upwardVelocity);
        pushInfo.upwardApplied = true;
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void StopAllPushes()
    {
        foreach (KeyValuePair<PlayerMovement, Whipper.PushInfo> pair in activePushes)
        {
            if (pair.Value != null && pair.Value.coroutine != null)
            {
                StopCoroutine(pair.Value.coroutine);
            }
        }

        activePushes.Clear();
    }

    private void OnDisable()
    {
        StopAllPushes();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 drawDirection = pushDirection;

        if (drawDirection.sqrMagnitude < 0.001f)
        {
            drawDirection = transform.forward;
            drawDirection.y = 0f;
        }

        if (drawDirection.sqrMagnitude < 0.001f)
        {
            drawDirection = Vector3.forward;
        }

        drawDirection.Normalize();

        Vector3 drawCenter = transform.position + drawDirection * pushBoxForwardOffset;
        Quaternion drawRotation = Quaternion.LookRotation(drawDirection, Vector3.up);

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(drawCenter, drawRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, PushBoxSize * 2f);

        Gizmos.matrix = oldMatrix;
    }
}