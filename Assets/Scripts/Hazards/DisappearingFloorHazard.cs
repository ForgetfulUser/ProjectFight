using System.Collections;
using UnityEngine;

public class DisappearingFloorHazard : BaseHazard
{
    public enum TriggerMode
    {
        Timer,
        Pressure
    }

    private enum HazardState
    {
        Active,
        WaitingForPressure,
        PressureDelay,
        Warning,
        Disappearing,
        Hidden
    }

    [Header("Mode")]
    public TriggerMode triggerMode = TriggerMode.Timer;

    [Header("Timing")]
    public float activeTime = 3f;
    public float pressureDelayTime = 0.5f;
    public float warningTime = 1f;
    public float hiddenTime = 2f;

    [Header("Animation")]
    public bool useDisappearAnimation = true;
    public Animator floorAnimator;
    public string disappearTriggerName = "Disappear";
    public string disappearStateName = "Disappear";
    public string idleStateName = "Idle";

    [Header("Warning Pulse")]
    public bool useWarningPulse = true;
    public float warningPulseAmount = 0.08f;
    public float warningPulseSpeed = 8f;

    [Header("Pressure Check")]
    public LayerMask pressureLayer;
    public Vector3 pressureCheckOffset = new Vector3(0f, 0.8f, 0f);
    public Vector3 pressureCheckHalfSize = new Vector3(1f, 1f, 1f);

    [Header("References")]
    [SerializeField] private Transform visualRender_TRSFM;
    [SerializeField] private Collider floorCollider;

    private float timer;
    private HazardState state;
    private Coroutine disappearRoutine;

    private Vector3 visualStartLocalScale;
    private bool hasSavedVisualStartScale;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        if (floorAnimator == null && visualRender_TRSFM != null)
        {
            floorAnimator = visualRender_TRSFM.GetComponent<Animator>();
        }

        if (floorCollider == null)
        {
            floorCollider = GetComponent<Collider>();
        }

        SaveVisualStartValues();
        RestoreTile();

        if (triggerMode == TriggerMode.Timer)
        {
            state = HazardState.Active;
            timer = activeTime;
        }
        else
        {
            state = HazardState.WaitingForPressure;
            timer = 0f;
        }
    }

    public override void UpdateHazard()
    {
        if (triggerMode == TriggerMode.Timer)
        {
            UpdateTimerMode();
        }
        else
        {
            UpdatePressureMode();
        }
    }

    public override void ResetHazard()
    {
        if (disappearRoutine != null)
        {
            StopCoroutine(disappearRoutine);
            disappearRoutine = null;
        }

        RestoreTile();

        if (triggerMode == TriggerMode.Timer)
        {
            state = HazardState.Active;
            timer = activeTime;
        }
        else
        {
            state = HazardState.WaitingForPressure;
            timer = 0f;
        }
    }

    private void UpdateTimerMode()
    {
        if (state == HazardState.Active)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                StartWarning();
            }
        }
        else if (state == HazardState.Warning)
        {
            timer -= Time.deltaTime;
            UpdateWarningPulse();

            if (timer <= 0f)
            {
                StartDisappearAnimation();
            }
        }
        else if (state == HazardState.Hidden)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                RestoreTile();

                state = HazardState.Active;
                timer = activeTime;
            }
        }
    }

    private void UpdatePressureMode()
    {
        if (state == HazardState.WaitingForPressure)
        {
            ResetWarningPulse();

            if (IsPressureDetected())
            {
                StartPressureDelay();
            }
        }
        else if (state == HazardState.PressureDelay)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                StartWarning();
            }
        }
        else if (state == HazardState.Warning)
        {
            timer -= Time.deltaTime;
            UpdateWarningPulse();

            if (timer <= 0f)
            {
                StartDisappearAnimation();
            }
        }
        else if (state == HazardState.Hidden)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                RestoreTile();

                state = HazardState.WaitingForPressure;
                timer = 0f;
            }
        }
    }

    private void StartPressureDelay()
    {
        state = HazardState.PressureDelay;
        timer = pressureDelayTime;
        ResetWarningPulse();
    }

    private void StartWarning()
    {
        state = HazardState.Warning;
        timer = warningTime;

        if (warningTime <= 0f)
        {
            StartDisappearAnimation();
        }
    }

    private void StartDisappearAnimation()
    {
        if (state == HazardState.Disappearing)
        {
            return;
        }

        state = HazardState.Disappearing;

        ResetWarningPulse();

        if (disappearRoutine != null)
        {
            StopCoroutine(disappearRoutine);
        }

        disappearRoutine = StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        if (useDisappearAnimation && floorAnimator != null)
        {
            floorAnimator.ResetTrigger(disappearTriggerName);
            floorAnimator.SetTrigger(disappearTriggerName);

            yield return null;

            while (!IsAnimatorInState(disappearStateName))
            {
                yield return null;
            }

            while (IsAnimatorInState(disappearStateName))
            {
                AnimatorStateInfo stateInfo = floorAnimator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.normalizedTime >= 1f && !floorAnimator.IsInTransition(0))
                {
                    break;
                }

                yield return null;
            }
        }

        HideTileImmediately();

        disappearRoutine = null;
    }

    private bool IsAnimatorInState(string stateName)
    {
        if (floorAnimator == null)
        {
            return false;
        }

        AnimatorStateInfo stateInfo = floorAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName);
    }

    private void HideTileImmediately()
    {
        state = HazardState.Hidden;
        timer = hiddenTime;

        if (floorCollider != null)
        {
            floorCollider.enabled = false;
        }

        SetTileVisible(false);
    }

    private void RestoreTile()
    {
        SetTileVisible(true);

        if (floorCollider != null)
        {
            floorCollider.enabled = true;
        }

        if (floorAnimator != null && !string.IsNullOrEmpty(idleStateName))
        {
            floorAnimator.Play(idleStateName, 0, 0f);
        }

        ResetWarningPulse();
    }

    private void SaveVisualStartValues()
    {
        if (visualRender_TRSFM == null)
        {
            return;
        }

        visualStartLocalScale = visualRender_TRSFM.localScale;
        hasSavedVisualStartScale = true;
    }

    private void UpdateWarningPulse()
    {
        if (!useWarningPulse)
        {
            return;
        }

        if (visualRender_TRSFM == null)
        {
            return;
        }

        if (!hasSavedVisualStartScale)
        {
            SaveVisualStartValues();
        }

        float pulse = 1f + Mathf.Sin(Time.time * warningPulseSpeed) * warningPulseAmount;
        visualRender_TRSFM.localScale = visualStartLocalScale * pulse;
    }

    private void ResetWarningPulse()
    {
        if (visualRender_TRSFM == null)
        {
            return;
        }

        if (!hasSavedVisualStartScale)
        {
            SaveVisualStartValues();
        }

        visualRender_TRSFM.localScale = visualStartLocalScale;
    }

    private bool IsPressureDetected()
    {
        Vector3 checkCenter = transform.position + pressureCheckOffset;

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            pressureCheckHalfSize,
            transform.rotation,
            pressureLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            PlayerMovement playerMovement = hit.GetComponentInParent<PlayerMovement>();

            if (playerMovement != null)
            {
                return true;
            }
        }

        return false;
    }

    private void SetTileVisible(bool visible)
    {
        if (visualRender_TRSFM != null)
        {
            visualRender_TRSFM.gameObject.SetActive(visible);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + pressureCheckOffset,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, pressureCheckHalfSize * 2f);

        Gizmos.matrix = oldMatrix;
    }
}