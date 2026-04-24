using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public HazardManager HazardManager;

    private void Awake()
    {
        HazardManager.WakeUpManager();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PlayerManager.StartManager(true);
        HazardManager.StartManager();
    }

    // Update is called once per frame
    private void Update()
    {
        PlayerManager.UpdateManager();
        HazardManager.UpdateMnager();
    }

    private void LateUpdate()
    {
        //Update UI here if needed
    }
}
