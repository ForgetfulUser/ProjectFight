using UnityEngine;

public class Whipper : BaseHazard
{
    [Header("Rotating Part")]
    [SerializeField] private Transform rotatingPart_TRSFM;

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

    [Header("Reset")]
    public bool resetWholeObjectTransform = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    private Vector3 rotatingPartStartLocalPosition;
    private Quaternion rotatingPartStartLocalRotation;
    private Vector3 rotatingPartStartLocalScale;

    private WhipperCollider whipperCollider;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        SaveStartTransform();
        SetupWhipper();
        ResetHazard();
    }

    public override void UpdateHazard()
    {
        RotateWhipper();
    }

    public override void ResetHazard()
    {
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

    private void SetupWhipper()
    {
        if (rotatingPart_TRSFM == null)
        {
            Debug.LogWarning("Whipper needs rotatingPart_TRSFM assigned.", gameObject);
            return;
        }

        whipperCollider = rotatingPart_TRSFM.GetComponentInChildren<WhipperCollider>();

        if (whipperCollider == null)
        {
            whipperCollider = rotatingPart_TRSFM.gameObject.AddComponent<WhipperCollider>();
        }

        whipperCollider.SetWhipper(this);

        Collider col = whipperCollider.GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("WhipperCollider object needs a Collider.", whipperCollider.gameObject);
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

        rotatingPart_TRSFM.Rotate(axis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
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
        if (rotatingPart_TRSFM == null)
        {
            return;
        }

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

        Vector3 fromCenter = playerRb.worldCenterOfMass - rotatingPart_TRSFM.position;
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