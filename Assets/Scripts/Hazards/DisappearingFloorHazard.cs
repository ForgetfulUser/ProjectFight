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
        Hidden
    }

    [Header("Mode")]
    public TriggerMode triggerMode = TriggerMode.Timer;

    [Header("Timing")]
    public float activeTime = 3f;
    public float pressureDelayTime = 0.5f;
    public float warningTime = 1f;
    public float hiddenTime = 2f;

    [Header("Pressure Check")]
    public LayerMask pressureLayer;
    public Vector3 pressureCheckOffset = new Vector3(0f, 0.4f, 0f);
    public Vector3 pressureCheckHalfSize = new Vector3(1f, 0.3f, 1f);

    [Header("Visual")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Material lavaMaterial;

    [SerializeField] Transform visualRender_TRSFM;

    private Renderer visualRenderer;
    private float timer;
    private HazardState state;

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        if (visualRender_TRSFM != null)
        {
            visualRenderer = visualRender_TRSFM.GetComponent<Renderer>();
            lavaMaterial = visualRenderer.material;
        }

        ResetHazard();
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
                HideTile();
            }
        }
        else if (state == HazardState.Hidden)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                ResetHazard();
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
                HideTile();
            }
        }
        else if (state == HazardState.Hidden)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                ResetHazard();
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
    }

    private void HideTile()
    {
        state = HazardState.Hidden;
        timer = hiddenTime;

        SetTileVisible(false);
    }

    private void RestoreTile()
    {
        SetTileVisible(true);
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
        if (visualRenderer != null)
        {
            visualRenderer.material.color = color;
        }
    }

    private void FlashWarning()
    {
        if (visualRenderer == null)
        {
            return;
        }

        float flash = Mathf.PingPong(Time.time * 6f, 1f);
        lavaMaterial.SetFloat("_Blend", flash);
        //visualRenderer.material.color = Color.Lerp(normalColor, warningColor, flash);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + pressureCheckOffset, pressureCheckHalfSize * 2f);
    }
}