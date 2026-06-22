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
    [Header("Removal")] // The manager checks if IsActive is false and if ShouldRemove is true. If this condition is met, the hazard will be removed
    public bool ShouldRemove = true;
    public bool IsActive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void StartHazard(HazardManager hazardManager)
    {
        this.hazardManager = hazardManager;
    }

    // Update is called once per frame
    public virtual void UpdateHazard()
    {
        if (IsActive == false) return;
    }

    public virtual void HitPlayer(PlayerMovement playerMovement, Vector3 force)
    {
        playerMovement.ApplyForce(force, stunDuration, forceMode);
        playerMovement.GetComponent<CharacterAudioController>().PlayClipByType(AudioType.TakeDamage);
    }

    public virtual void FixedUpdateHazard()
    {
        if (IsActive == false) return;
    }

    public virtual void ResetHazard()
    {
        IsActive = true;
    }

    public virtual void SetForRemoval()
    {
        IsActive = false;
    }
}
