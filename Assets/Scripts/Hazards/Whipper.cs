using UnityEngine;

public class Whipper : BaseHazard
{
    [Header("Rotating Part")]
    public Transform rotatingPart;

    [Header("Rotation")]
    public Vector3 localRotationAxis = Vector3.up;
    public float rotationSpeed = 180f;

    [Header("Player Detection")]
    public LayerMask playerLayer;

    [Header("Push")]
    public float pushForce = 35f;
    public float outwardForceRatio = 0.25f;
    public float upwardForce = 0f;
    public float maxHorizontalSpeed = 12f;
    public bool invertPushDirection = false;

    private WhipperCollider whipperCollider;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);
        SetupWhipper();
    }

    public override void UpdateHazard()
    {
        RotateWhipper();
    }

    private void Awake()
    {
        SetupWhipper();
    }

    private void SetupWhipper()
    {
        if (rotatingPart == null)
        {
            Debug.LogWarning("Whipper needs a rotatingPart assigned.", gameObject);
            return;
        }

        whipperCollider = rotatingPart.GetComponent<WhipperCollider>();

        if (whipperCollider == null)
        {
            whipperCollider = rotatingPart.gameObject.AddComponent<WhipperCollider>();
        }

        whipperCollider.SetWhipper(this);

        Collider col = rotatingPart.GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("RotatingPart needs a Collider for Whipper to push the player.", rotatingPart.gameObject);
        }
    }

    private void RotateWhipper()
    {
        if (rotatingPart == null)
        {
            return;
        }

        Vector3 axis = localRotationAxis;

        if (axis.sqrMagnitude < 0.001f)
        {
            axis = Vector3.up;
        }

        rotatingPart.Rotate(axis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
    }

    public void TryPush(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            return;
        }

        Rigidbody playerRb = other.GetComponentInParent<Rigidbody>();

        if (playerRb == null)
        {
            return;
        }

        PushPlayer(playerRb);
    }

    private void PushPlayer(Rigidbody playerRb)
    {
        if (rotatingPart == null)
        {
            return;
        }

        Vector3 axisWorld = rotatingPart.TransformDirection(localRotationAxis.normalized);

        if (axisWorld.sqrMagnitude < 0.001f)
        {
            axisWorld = Vector3.up;
        }

        axisWorld.Normalize();

        Vector3 fromCenter = playerRb.worldCenterOfMass - rotatingPart.position;

        Vector3 radialDirection = fromCenter - Vector3.Project(fromCenter, axisWorld);

        if (radialDirection.sqrMagnitude < 0.001f)
        {
            radialDirection = rotatingPart.right;
        }

        radialDirection.Normalize();

        float directionSign = Mathf.Sign(rotationSpeed);

        if (invertPushDirection)
        {
            directionSign *= -1f;
        }

        Vector3 tangentDirection = Vector3.Cross(axisWorld, radialDirection).normalized * directionSign;

        Vector3 finalPushDirection = tangentDirection + radialDirection * outwardForceRatio;

        if (finalPushDirection.sqrMagnitude < 0.001f)
        {
            finalPushDirection = tangentDirection;
        }

        finalPushDirection.Normalize();

        Vector3 currentHorizontalVelocity = new Vector3(
            playerRb.linearVelocity.x,
            0f,
            playerRb.linearVelocity.z
        );

        float speedInPushDirection = Vector3.Dot(currentHorizontalVelocity, finalPushDirection);

        if (speedInPushDirection < maxHorizontalSpeed)
        {
            playerRb.AddForce(finalPushDirection * pushForce, ForceMode.Acceleration);
        }

        if (upwardForce > 0f)
        {
            playerRb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}