using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whipper : BaseHazard
{
    public class PushInfo
    {
        public Rigidbody rb;
        public PlayerMovement playerMovement;
        public Vector3 pushDirection;
        public float timer;
        public Coroutine coroutine;
        public bool upwardApplied;
    }

    [Header("Rotating Part")]
    [SerializeField] private Transform rotatingPart_TRSFM;

    [Header("Hitbox")]
    [SerializeField] private WhipperCollider whipperCollider;

    [Header("Rotation")]
    public Vector3 localRotationAxis = Vector3.up;
    public float rotationSpeed = 150f;

    [Header("Player Detection")]
    public LayerMask playerLayer;

    [Header("Sweep Push")]
    public float pushForce = 55f;
    public float outwardForceRatio = 0.25f;
    public float maxPushSpeed = 26f;
    public bool invertPushDirection = false;

    [Header("Upward Launch")]
    public float upwardVelocity = 4f;
    public bool applyUpwardOnlyOnNewHit = true;

    [Header("After-Hit Coroutine Push")]
    public float pushDuration = 0.45f;
    public bool fadePushOverTime = true;
    public float playerStunTime = 0.18f;

    [Header("Reset")]
    public bool resetWholeObjectTransform = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    private Vector3 rotatingPartStartLocalPosition;
    private Quaternion rotatingPartStartLocalRotation;
    private Vector3 rotatingPartStartLocalScale;

    private readonly Dictionary<PlayerMovement, PushInfo> activePushes = new Dictionary<PlayerMovement, PushInfo>();

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        SaveStartTransform();
        ResetHazard();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 forceDir = GetWhipperPushDirection(collision.gameObject.GetComponent<Rigidbody>());
            Debug.Log(forceDir * forceAmount + " on " + collision.gameObject.GetComponent<Rigidbody>());
            collision.gameObject.GetComponent<Rigidbody>().AddForce(forceDir * forceAmount, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
    }

    public override void FixedUpdateHazard()
    {
        RotateWhipper();
    }

    public override void ResetHazard()
    {
        StopAllPushCoroutines();

        if (resetWholeObjectTransform)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            transform.localScale = startScale;
        }

        if (rotatingPart_TRSFM != null)
        {
            rotatingPart_TRSFM.localPosition = rotatingPartStartLocalPosition;
            rotatingPart_TRSFM.localRotation = rotatingPartStartLocalRotation;
            rotatingPart_TRSFM.localScale = rotatingPartStartLocalScale;
            rotatingPart_TRSFM.gameObject.SetActive(true);
        }
    }

    private void SaveStartTransform()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;

        if (rotatingPart_TRSFM != null)
        {
            rotatingPartStartLocalPosition = rotatingPart_TRSFM.localPosition;
            rotatingPartStartLocalRotation = rotatingPart_TRSFM.localRotation;
            rotatingPartStartLocalScale = rotatingPart_TRSFM.localScale;
        }
    }

    private void RotateWhipper()
    {
        if (rotatingPart_TRSFM == null)
        {
            return;
        }

        Vector3 axis = localRotationAxis;

        if (axis.sqrMagnitude < 0.001f)
        {
            axis = Vector3.up;
        }
        float rotateInput = Input.GetAxis("Horizontal"); // -1 to 1
        float rotationSpeed = 100f;

        // Create a small rotation this frame
        Quaternion deltaRotation = Quaternion.Euler(0f, rotateInput * rotationSpeed * Time.fixedDeltaTime, 0f);

        // Apply it to current rotation
        Rigidbody rb = rotatingPart_TRSFM.GetComponent<Rigidbody>();
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    public void RegisterHit(Collider other)
    {
        Debug.Log("Registerring");
        PlayerMovement playerMovement = other.GetComponentInParent<PlayerMovement>();

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

        bool colliderLayerMatches = IsInLayerMask(other.gameObject.layer, playerLayer);
        bool playerLayerMatches = IsInLayerMask(playerMovement.gameObject.layer, playerLayer);

        if (!colliderLayerMatches && !playerLayerMatches)
        {
            return;
        }

        Vector3 pushDirection = GetWhipperPushDirection(targetRb);

        StartPush(
            targetRb,
            playerMovement,
            pushDirection,
            true
        );
    }

    public PushInfo StartPush(
        Rigidbody targetRb,
        PlayerMovement playerMovement,
        Vector3 pushDirection,
        bool applyUpward
    )
    {
        if (targetRb == null || playerMovement == null)
        {
            return null;
        }

        if (pushDirection.sqrMagnitude < 0.001f)
        {
            pushDirection = playerMovement.transform.position - transform.position;
            pushDirection.y = 0f;

            if (pushDirection.sqrMagnitude < 0.001f)
            {
                pushDirection = transform.forward;
            }
        }

        pushDirection.y = 0f;
        pushDirection.Normalize();

        if (activePushes.TryGetValue(playerMovement, out PushInfo existingPush))
        {
            existingPush.rb = targetRb;
            existingPush.playerMovement = playerMovement;
            existingPush.pushDirection = pushDirection;
            existingPush.timer = pushDuration;

            if (!applyUpwardOnlyOnNewHit && applyUpward)
            {
                ApplyUpwardLaunch(existingPush);
            }

            return existingPush;
        }

        PushInfo pushInfo = new PushInfo();
        pushInfo.rb = targetRb;
        pushInfo.playerMovement = playerMovement;
        pushInfo.pushDirection = pushDirection;
        pushInfo.timer = pushDuration;
        pushInfo.upwardApplied = false;

        pushInfo.coroutine = StartCoroutine(PushPlayerCoroutine(pushInfo));

        activePushes.Add(playerMovement, pushInfo);

        if (applyUpward)
        {
            ApplyUpwardLaunch(pushInfo);
        }

        return pushInfo;
    }

    public bool TryGetPushInfo(PlayerMovement playerMovement, out PushInfo pushInfo)
    {
        return activePushes.TryGetValue(playerMovement, out pushInfo);
    }

    public void StopPush(PlayerMovement playerMovement)
    {
        if (playerMovement == null)
        {
            return;
        }

        if (!activePushes.TryGetValue(playerMovement, out PushInfo pushInfo))
        {
            return;
        }

        if (pushInfo != null && pushInfo.coroutine != null)
        {
            StopCoroutine(pushInfo.coroutine);
        }

        activePushes.Remove(playerMovement);
    }

    private IEnumerator PushPlayerCoroutine(PushInfo pushInfo)
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

    private void ApplyPushVelocity(PushInfo pushInfo, float forceMultiplier)
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

        if (playerStunTime > 0f)
        {
            pushInfo.playerMovement.Stun(playerStunTime);
        }
    }

    private void ApplyUpwardLaunch(PushInfo pushInfo)
    {
        if (pushInfo == null || pushInfo.playerMovement == null)
        {
            return;
        }

        if (upwardVelocity <= 0f)
        {
            return;
        }

        if (applyUpwardOnlyOnNewHit && pushInfo.upwardApplied)
        {
            return;
        }

        pushInfo.playerMovement.AddVerticalVelocity(upwardVelocity);
        pushInfo.upwardApplied = true;
    }

    private Vector3 GetWhipperPushDirection(Rigidbody targetRb)
    {
        Vector3 axis = localRotationAxis;

        if (axis.sqrMagnitude < 0.001f)
        {
            axis = Vector3.up;
        }

        Vector3 axisWorld = rotatingPart_TRSFM.TransformDirection(axis.normalized);

        if (axisWorld.sqrMagnitude < 0.001f)
        {
            axisWorld = Vector3.up;
        }

        axisWorld.Normalize();

        Vector3 fromCenter = targetRb.worldCenterOfMass - rotatingPart_TRSFM.position;

        Vector3 radialDirection = fromCenter - Vector3.Project(fromCenter, axisWorld);

        if (radialDirection.sqrMagnitude < 0.001f)
        {
            radialDirection = rotatingPart_TRSFM.right;
        }

        radialDirection.Normalize();

        float directionSign = Mathf.Sign(rotationSpeed);

        if (invertPushDirection)
        {
            directionSign *= -1f;
        }

        Vector3 tangentDirection = Vector3.Cross(axisWorld, radialDirection).normalized * directionSign;

        Vector3 finalDirection = tangentDirection + radialDirection * outwardForceRatio;

        finalDirection.y = 0f;

        if (finalDirection.sqrMagnitude < 0.001f)
        {
            finalDirection = tangentDirection;
        }

        finalDirection.Normalize();

        return finalDirection;
    }

    private void StopAllPushCoroutines()
    {
        foreach (KeyValuePair<PlayerMovement, PushInfo> pair in activePushes)
        {
            if (pair.Value != null && pair.Value.coroutine != null)
            {
                StopCoroutine(pair.Value.coroutine);
            }
        }

        activePushes.Clear();
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}