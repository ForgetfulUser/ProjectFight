using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    public List<BaseHazard> Hazards = new List<BaseHazard>();

    public void WakeUpManager()
    {
        foreach(var type in FindObjectsByType(typeof(BaseHazard), FindObjectsSortMode.None))
        {
            Hazards.Add(type.GetComponent<BaseHazard>());
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
