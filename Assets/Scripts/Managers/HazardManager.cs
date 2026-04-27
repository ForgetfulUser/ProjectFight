using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    public List<BaseHazard> Hazards = new List<BaseHazard>();

    public virtual void WakeUpManager()
    {
        Hazards.Clear();

        foreach (var hazard in FindObjectsByType<BaseHazard>(FindObjectsSortMode.None))
        {
            Hazards.Add(hazard);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void StartManager()
    {
        foreach(var hazard in Hazards)
        {
            hazard.StartHazard(this);
        }
    }

    // Update is called once per frame
    public virtual void UpdateManager()
    {
        foreach(var haz in Hazards)
        {
            haz.UpdateHazard();
        }
    }

    public virtual void FixedUpdateManager()
    {
        foreach (var haz in Hazards)
        {
            haz.FixedUpdateHazard();
        }
    }

    public virtual void ResetHazards()
    {
        foreach (var haz in Hazards)
        {
            haz.ResetHazard();
        }
    }
}
