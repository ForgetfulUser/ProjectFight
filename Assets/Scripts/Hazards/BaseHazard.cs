using UnityEngine;

public class BaseHazard : MonoBehaviour
{
    protected HazardManager hazardManager;
    [Header("Force")]
    public bool doesApplyForce;
    public ForceMode forceMode;
    public float forceAmount; 
    [Range(0f, 1f)]
    public float angleOfForce;
    public float upwardForceMultiplier;
    public float forceCooldown;
    [Header("Stun")]
    public float stunDuration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void StartHazard(HazardManager hazardManager)
    {
        this.hazardManager = hazardManager;
    }

    // Update is called once per frame
    public virtual void UpdateHazard()
    {

    }

    public virtual void HitPlayer(PlayerMovement playerMovement, Vector3 force)
    {
        playerMovement.ApplyForce(force, stunDuration, forceMode);
    }

    public virtual void FixedUpdateHazard()
    {

    }

    public virtual void ResetHazard()
    {

    }
}
