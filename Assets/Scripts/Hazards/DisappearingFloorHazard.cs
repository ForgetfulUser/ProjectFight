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
    public float warningTime = 0f;
    public float hiddenTime = 2f;

    [Header("Animation")]
    public bool useDisappearAnimation = true;
    public Animator floorAnimator;
    public string disappearTriggerName = "Disappear";
    public string disappearStateName = "Disappear";
    public string idleStateName = "Idle";

    [Header("Pressure Check")]
    public LayerMask pressureLayer;
    public Vector3 pressureCheckOffset = new Vector3(0f, 0.4f, 0f);
    public Vector3 pressureCheckHalfSize = new Vector3(1f, 0.3f, 1f);

    [Header("Visual")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [SerializeField] private Transform visualRender_TRSFM;
    [SerializeField] private Collider floorCollider;

    private float timer;
    private HazardState state;
    private Coroutine disappearRoutine;

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
            FlashWarning();

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
            SetTileColor(normalColor);

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
            FlashWarning();

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
        SetTileColor(normalColor);
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

            // Wait one frame so Animator has time to enter the new state.
            yield return null;

            // Wait until the animator actually enters the disappear state.
            while (!IsAnimatorInState(disappearStateName))
            {
                yield return null;
            }

            // Wait until the disappear state finishes playing.
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
        if (visualRender_TRSFM != null)
        {
            visualRender_TRSFM.gameObject.SetActive(true);
        }

        if (floorCollider != null)
        {
            floorCollider.enabled = true;
        }

        if (floorAnimator != null && !string.IsNullOrEmpty(idleStateName))
        {
            floorAnimator.Play(idleStateName, 0, 0f);
        }

        SetTileColor(normalColor);
    }

    private bool IsPressureDetected()
    {
        Vector3 checkCenter = transform.position + pressureCheckOffset;

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            pressureCheckHalfSize,
            Quaternion.identity,
            pressureLayer
        );

        return hits.Length > 0;
    }

    private void SetTileVisible(bool visible)
    {
        if (visualRender_TRSFM != null)
        {
            visualRender_TRSFM.gameObject.SetActive(visible);
        }
    }

    private void SetTileColor(Color color)
    {
        if (visualRender_TRSFM == null)
        {
            return;
        }

        Renderer renderer = visualRender_TRSFM.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private void FlashWarning()
    {
        if (visualRender_TRSFM == null)
        {
            return;
        }

        Renderer renderer = visualRender_TRSFM.GetComponent<Renderer>();

        if (renderer == null)
        {
            return;
        }

        float flash = Mathf.PingPong(Time.time * 6f, 1f);
        renderer.material.color = Color.Lerp(normalColor, warningColor, flash);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + pressureCheckOffset, pressureCheckHalfSize * 2f);
    }
}