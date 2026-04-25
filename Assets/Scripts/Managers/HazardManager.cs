using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    public List<BaseHazard> Hazards = new List<BaseHazard>();

    public void WakeUpManager()
    {
        Hazards.Clear();

        foreach (var hazard in FindObjectsByType<BaseHazard>(FindObjectsSortMode.None))
        {
            Hazards.Add(hazard);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartManager()
    {
        foreach(var hazard in Hazards)
        {
            hazard.StartHazard(this);
        }
    }

    // Update is called once per frame
    public void UpdateMnager()
    {
        foreach(var haz in Hazards)
        {
            haz.UpdateHazard();
        }
    }
}
