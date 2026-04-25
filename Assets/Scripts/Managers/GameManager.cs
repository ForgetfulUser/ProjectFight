using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public HazardManager HazardManager;

    protected virtual void Awake()
    {
        HazardManager.WakeUpManager();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        PlayerManager.StartManager(true);
        HazardManager.StartManager();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        PlayerManager.UpdateManager();
        HazardManager.UpdateManager();
    }

    protected virtual void LateUpdate()
    {
        //Update UI here if needed
    }
}
