using UnityEngine;

public class BaseHazard : MonoBehaviour
{
    private HazardManager hazardManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartHazard(HazardManager hazardManager)
    {
        this.hazardManager = hazardManager;
    }

    // Update is called once per frame
    public void UpdateHazard()
    {
        
    }
}
