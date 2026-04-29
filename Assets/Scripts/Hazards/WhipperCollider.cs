using UnityEngine;

public class WhipperCollider : MonoBehaviour
{
    private Whipper whipper;
    private Vector3 localRotationAxis;
    private bool invertPushDirection;
    private float outwardForceRatio;
    private float rotationSpeed;
    private float angleOfForce;
    private float forceAmount;

    public void SetWhipper(Whipper whipper, float angleOfForce, float forceAmount)
    {
        this.whipper = whipper;
        rotationSpeed = whipper.rotationSpeed;
        localRotationAxis = whipper.localRotationAxis;
        invertPushDirection = whipper.invertPushDirection;
        outwardForceRatio = whipper.outwardForceRatio;
        this.angleOfForce = angleOfForce;
        this.forceAmount = forceAmount;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 forceDir = GetWhipperPushDirection(collision.gameObject.GetComponent<Rigidbody>());
            whipper.HitPlayer(collision.gameObject.GetComponent<PlayerMovement>(), forceDir * forceAmount);
        }
    }

    private Vector3 GetWhipperPushDirection(Rigidbody targetRb)
    {
        Vector3 axis = localRotationAxis;

        if (axis.sqrMagnitude < 0.001f)
        {
            axis = Vector3.up;
        }

        Vector3 axisWorld = transform.TransformDirection(axis.normalized);

        if (axisWorld.sqrMagnitude < 0.001f)
        {
            axisWorld = Vector3.up;
        }

        axisWorld.Normalize();

        Vector3 fromCenter = targetRb.worldCenterOfMass - transform.position;

        Vector3 radialDirection = fromCenter - Vector3.Project(fromCenter, axisWorld);

        if (radialDirection.sqrMagnitude < 0.001f)
        {
            radialDirection = transform.right;
        }

        radialDirection.Normalize();

        float directionSign = Mathf.Sign(rotationSpeed);

        if (invertPushDirection)
        {
            directionSign *= -1f;
        }

        Vector3 tangentDirection = Vector3.Cross(axisWorld, radialDirection).normalized * directionSign;

        Vector3 finalDirection = tangentDirection + radialDirection * outwardForceRatio;

        finalDirection.y = angleOfForce;

        if (finalDirection.sqrMagnitude < 0.001f)
        {
            finalDirection = tangentDirection;
        }

        finalDirection.Normalize();

        return finalDirection;
    }

}