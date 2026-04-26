using UnityEngine;

public class WhipperCollider : MonoBehaviour
{
    private Whipper whipper;

    public void SetWhipper(Whipper whipper)
    {
        this.whipper = whipper;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (whipper != null)
        {
            whipper.TryPush(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (whipper != null)
        {
            whipper.TryPush(other);
        }
    }
}