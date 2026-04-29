using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class PlayerPushComponent : MonoBehaviour
{
    [Header("Push Force")]
    public float pushForce = 55f;
    public float upwardVelocity = 3f;
    public float maxPushSpeed = 26f;
    private Vector3 pushDirection;

    [Header("Push Detection")]
    public Vector3 PushBoxSize = new Vector3(1f, 0.75f, 1f);
    public float pushBoxForwardOffset = 1f;
    public LayerMask PlayerLayer;
    private bool tryPush;

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
        if (tryPush == false) return;

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

        foreach (Collider hit in hits)
        {
            // Calculate direction away from this object
            if(hit.gameObject == gameObject) continue;
            hit.attachedRigidbody.AddForce(pushDirection * Time.deltaTime * pushForce, ForceMode.Impulse);
            //RegisterHit(hit);
        }
    }

    private Vector3 GetPushDirection(Vector2 moveDir)
    {
        Vector3 direction = new Vector3(
            moveDir.x,
            0.5f,
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