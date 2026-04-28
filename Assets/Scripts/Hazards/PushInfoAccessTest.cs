using UnityEngine;

public class PushInfoAccessTest : MonoBehaviour
{
    public Whipper whipper;

    private Whipper.PushInfo testPushInfo;

    private void Start()
    {
        if (whipper != null)
        {
            Debug.Log("PushInfo is accessible.");
        }
    }
}