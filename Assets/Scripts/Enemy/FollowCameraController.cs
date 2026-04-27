using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);
    public float distance = 8f;
    public float minDistance = 3f;
    public float maxDistance = 15f;
    public float zoomSpeed = 1f;
    public float followSmoothTime = 0.08f;

    [Header("Rotation")]
    public float yaw = 0f;
    public float pitch = 35f;
    public float minPitch = 10f;
    public float maxPitch = 80f;
    public float keyboardRotateSpeed = 90f;

    [Header("Input")]
    public bool requireRightMouseToRotate = true;
    public bool allowScrollZoom = true;

    private Vector3 followVelocity;

    private void Start()
    {
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        UpdateCameraPositionInstant();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleRotationInput();
        HandleZoomInput();
        UpdateCameraPositionSmooth();
    }

    private void HandleRotationInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        bool canRotate = true;

        if (requireRightMouseToRotate)
        {
            canRotate = Mouse.current != null && Mouse.current.rightButton.isPressed;
        }

        if (!canRotate)
        {
            return;
        }

        float yawInput = 0f;
        float pitchInput = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            yawInput -= 1f;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            yawInput += 1f;
        }

        if (Keyboard.current.upArrowKey.isPressed)
        {
            pitchInput -= 1f;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            pitchInput += 1f;
        }

        yaw += yawInput * keyboardRotateSpeed * Time.deltaTime;
        pitch += pitchInput * keyboardRotateSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleZoomInput()
    {
        if (!allowScrollZoom)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (Mathf.Abs(scroll.y) <= 0.01f)
        {
            return;
        }

        distance -= Mathf.Sign(scroll.y) * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void UpdateCameraPositionSmooth()
    {
        Vector3 desiredPosition = GetDesiredCameraPosition();

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            followSmoothTime
        );

        LookAtTarget();
    }

    private void UpdateCameraPositionInstant()
    {
        if (target == null)
        {
            return;
        }

        transform.position = GetDesiredCameraPosition();
        LookAtTarget();
    }

    private Vector3 GetDesiredCameraPosition()
    {
        Vector3 targetPosition = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -distance);

        return targetPosition + cameraOffset;
    }

    private void LookAtTarget()
    {
        Vector3 lookTarget = target.position + targetOffset;
        Vector3 lookDirection = lookTarget - transform.position;

        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
    }
}