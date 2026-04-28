using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class PlayerPushComponent : MonoBehaviour
{
    [Header("Push Detection")]
    public Vector3 PushBoxSize;
    public LayerMask PlayerLayer;
    private bool tryPush;

    [Header("Push Force")]
    public float pushForce = 55f;
    public float maxPushSpeed = 26f;
    private Vector3 pushDirection;

    [Header("After-Hit Coroutine Push")]
    public float pushDuration = 0.45f;
    public bool fadePushOverTime = true;
    public float stunTime = 0.18f;

    private readonly Dictionary<PlayerMovement, Whipper.PushInfo> activePushes = new Dictionary<PlayerMovement, Whipper.PushInfo>();

    public void Attack(InputAction.CallbackContext context)
    {
        Debug.Log("Attack");
        tryPush = context.performed;
        if (tryPush == false) pushDirection = Vector3.zero;
    }

    public void UpdatePushComponent(Vector2 moveDir)
    {
        if (tryPush == false && activePushes.Count == 0) return;
        Debug.Log("Try Push");

        // Make Push Box
        pushDirection = new Vector3(
            moveDir.x,
            0,
            moveDir.y / 2
            );

        Vector3 boxPosition = transform.position + pushDirection;

        Collider[] hits = Physics.OverlapBox(
            boxPosition,
            PushBoxSize,
            Quaternion.identity,
            PlayerLayer
        );
        if (hits.Length > 0) Debug.Log(hits.Length + " hits foudn"); 
        // Stop player Coroutines
        StopAllCoroutines();

        // Apply Push
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                RegisterHit(hit);
            }
        }
    }

    private void RegisterHit(Collider hit)
    {
        Debug.Log("Registering Hit: " + hit.name);
        PlayerMovement playerMovement = hit.GetComponentInParent<PlayerMovement>();

        if (playerMovement == null)
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
        
        //Vector3 pushDirection = Vector3.zero;

        if (activePushes.TryGetValue(playerMovement, out Whipper.PushInfo existingPush))
        {
            existingPush.pushDirection = pushDirection;
            existingPush.timer = pushDuration;
            return;
        }
        Debug.Log("Applying Push");
        Whipper.PushInfo pushInfo = new Whipper.PushInfo();
        pushInfo.rb = targetRb;
        pushInfo.playerMovement = playerMovement;
        pushInfo.pushDirection = pushDirection;
        pushInfo.timer = pushDuration;
        pushInfo.coroutine = StartCoroutine(PushPlayerCoroutine(pushInfo));

        activePushes.Add(playerMovement, pushInfo);
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

        Vector3 pushDirection = pushInfo.pushDirection;

        if (pushDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        pushDirection.Normalize();

        Vector3 horizontalVelocity = new Vector3(
            pushInfo.rb.linearVelocity.x,
            0f,
            pushInfo.rb.linearVelocity.z
        );

        float speedInPushDirection = Vector3.Dot(horizontalVelocity, pushDirection);

        if (speedInPushDirection < maxPushSpeed)
        {
            Vector3 velocityChange =
                pushDirection * pushForce * forceMultiplier * Time.deltaTime;

            pushInfo.playerMovement.AddExternalVelocity(velocityChange);
        }

        if (stunTime > 0f)
        {
            pushInfo.playerMovement.Stun(stunTime);
        }

        Debug.Log("Comle");
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + pushDirection, PushBoxSize);
    }
}
