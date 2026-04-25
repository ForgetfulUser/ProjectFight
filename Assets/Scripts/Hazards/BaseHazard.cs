using UnityEngine;

public class BaseHazard : MonoBehaviour
{
    protected HazardManager hazardManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void StartHazard(HazardManager hazardManager)
    {
        this.hazardManager = hazardManager;
    }

    // Update is called once per frame
    public virtual void UpdateHazard()
    {

    }

    public virtual void ResetHazard()
    {

    }
}
