using UnityEngine;

public class DisappearingFloorHazard : BaseHazard
{
    [Header("Timing")]
    public float activeTime = 3f;
    public float warningTime = 1f;
    public float hiddenTime = 2f;

    [Header("Visual")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    private Renderer rend;
    private Collider col;

    private float timer;
    private int state;

    // state 0 = active
    // state 1 = warning
    // state 2 = hidden

    public override void StartHazard(HazardManager hazardManager)
    {
        base.StartHazard(hazardManager);

        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        timer = activeTime;
        state = 0;

        SetTileVisible(true);
        SetTileColor(normalColor);
    }

    public override void UpdateHazard()
    {
        timer -= Time.deltaTime;

        if (state == 0)
        {
            if (timer <= 0)
            {
                state = 1;
                timer = warningTime;
            }
        }
        else if (state == 1)
        {
            FlashWarning();

            if (timer <= 0)
            {
                state = 2;
                timer = hiddenTime;
                SetTileVisible(false);
            }
        }
        else if (state == 2)
        {
            if (timer <= 0)
            {
                state = 0;
                timer = activeTime;
                SetTileVisible(true);
                SetTileColor(normalColor);
            }
        }
    }

    private void SetTileVisible(bool visible)
    {
        if (rend != null)
        {
            rend.enabled = visible;
        }

        if (col != null)
        {
            col.enabled = visible;
        }
    }

    private void SetTileColor(Color color)
    {
        if (rend != null)
        {
            rend.material.color = color;
        }
    }

    private void FlashWarning()
    {
        if (rend == null)
        {
            return;
        }

        float flash = Mathf.PingPong(Time.time * 6f, 1f);
        rend.material.color = Color.Lerp(normalColor, warningColor, flash);
    }
}