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
    private WhipperCollider whipperCollider;

    [Header("Rotation")]
    public Vector3 localRotationAxis = Vector3.up;
    public float rotationSpeed = 150f;

    [Header("Player Detection")]
    public LayerMask playerLayer;

    [Header("Sweep Push")]
    public float outwardForceRatio = 0.25f;
    public float maxPushSpeed = 26f;
    public bool invertPushDirection = false;

    [Header("Reset")]
    public bool resetWholeObjectTransform = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    private Vector3 rotatingPartStartLocalPosition;
    private Quaternion rotatingPartStartLocalRotation;
    private Vector3 rotatingPartStartLocalScale;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);
        whipperCollider = rotatingPart_TRSFM.GetComponent<WhipperCollider>();
        whipperCollider.SetWhipper(this, angleOfForce, forceAmount);
        SaveStartTransform();
        ResetHazard();
    }

    public override void FixedUpdateHazard()
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
        float rotateInput = 1f;// Input.GetAxis("Horizontal"); // -1 to 1
        float rotationSpeed = 100f;

        // Create a small rotation this frame
        Quaternion deltaRotation = Quaternion.Euler(0f, rotateInput * rotationSpeed * Time.fixedDeltaTime, 0f);

        // Apply it to current rotation
        Rigidbody rb = rotatingPart_TRSFM.GetComponent<Rigidbody>();
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}