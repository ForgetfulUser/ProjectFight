using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCameraController : MonoBehaviour
{
    [Header("Look")]
    public float mouseSensitivity = 0.15f;
    public bool invertY = false;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float fastMoveMultiplier = 3f;
    public float scrollSpeedStep = 2f;
    public float minMoveSpeed = 1f;
    public float maxMoveSpeed = 50f;

    private float yaw;
    private float pitch;
    private bool isLooking;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        HandleLookToggle();
        HandleLook();
        HandleMovement();
        HandleSpeedScroll();
    }

    private void HandleLookToggle()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isLooking = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isLooking = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleLook()
    {
        if (!isLooking)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity;

        if (invertY)
        {
            pitch += mouseDelta.y * mouseSensitivity;
        }
        else
        {
            pitch -= mouseDelta.y * mouseSensitivity;
        }

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            moveDir += transform.forward;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            moveDir -= transform.forward;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            moveDir += transform.right;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            moveDir -= transform.right;
        }

        if (Keyboard.current.eKey.isPressed)
        {
            moveDir += Vector3.up;
        }

        if (Keyboard.current.qKey.isPressed)
        {
            moveDir -= Vector3.up;
        }

        if (moveDir.sqrMagnitude > 1f)
        {
            moveDir.Normalize();
        }

        float currentSpeed = moveSpeed;

        if (Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed *= fastMoveMultiplier;
        }

        transform.position += moveDir * currentSpeed * Time.deltaTime;
    }

    private void HandleSpeedScroll()
    {
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (Mathf.Abs(scroll.y) <= 0.01f)
        {
            return;
        }

        moveSpeed += Mathf.Sign(scroll.y) * scrollSpeedStep;
        moveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);
    }
}