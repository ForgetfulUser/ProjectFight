using UnityEngine;
using UnityEngine.InputSystem;

public class InfectedPlayerAttackComponent : MonoBehaviour
{
    [Header("Push Force")]
    public float pushForce = 55f;
    public float upwardVelocity = 3f;
    private Vector3 attackDirection;

    [Header("Attack Detection")]
    public Vector3 attackBoxSize = new Vector3(1f, 0.75f, 1f);
    public float attackBoxForwardOffset = 1f;
    public LayerMask playerLayer;
    private bool tryAttack;

    [Header("Push")]
    public float stunTime = 0.18f;
    public ForceMode pushForceMode = ForceMode.Impulse;

    [Header("Infection")]
    public bool infectedCanInfectHumans = true;
    public bool humanCanPushPlayers = true;
    public bool winnerCanAttack = false;

    [Header("References")]
    public PlayerMovement playerMovement;
    public PlayerInfection playerInfection;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        if (playerInfection == null)
        {
            playerInfection = GetComponentInParent<PlayerInfection>();
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        tryAttack = context.performed;

        if (tryAttack == false)
        {
            attackDirection = Vector3.zero;
        }
    }

    public void UpdateAttackComponent(Vector2 moveDir)
    {
        if (!tryAttack)
        {
            return;
        }

        tryAttack = false;

        if (playerInfection == null)
        {
            playerInfection = GetComponentInParent<PlayerInfection>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        if (playerInfection == null)
        {
            return;
        }

        if (playerInfection.IsWinner && !winnerCanAttack)
        {
            return;
        }

        attackDirection = GetAttackDirection(moveDir);

        Vector3 boxPosition = transform.position + attackDirection * attackBoxForwardOffset;
        Quaternion boxRotation = Quaternion.LookRotation(attackDirection, Vector3.up);

        Collider[] hits = Physics.OverlapBox(
            boxPosition,
            attackBoxSize,
            boxRotation,
            playerLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            PlayerMovement targetMovement = hit.GetComponentInParent<PlayerMovement>();

            if (targetMovement == null)
            {
                continue;
            }

            if (targetMovement == playerMovement)
            {
                continue;
            }

            if (playerInfection.IsHuman)
            {
                TryHumanPush(targetMovement);
            }
            else if (playerInfection.IsInfected)
            {
                TryInfectedInfect(hit);
            }
        }
    }

    private void TryHumanPush(PlayerMovement targetMovement)
    {
        if (!humanCanPushPlayers)
        {
            return;
        }

        if (targetMovement == null)
        {
            return;
        }

        targetMovement.ApplyForce(attackDirection * pushForce, stunTime, pushForceMode);

        if (showDebugLogs)
        {
            Debug.Log(name + " pushed " + targetMovement.name);
        }
    }

    private void TryInfectedInfect(Collider hit)
    {
        if (!infectedCanInfectHumans)
        {
            return;
        }

        if (playerInfection == null || !playerInfection.IsInfected)
        {
            return;
        }

        if (!playerInfection.CanInfect)
        {
            return;
        }

        PlayerInfection targetInfection = hit.GetComponentInParent<PlayerInfection>();

        if (targetInfection == null)
        {
            return;
        }

        if (targetInfection == playerInfection)
        {
            return;
        }

        if (!targetInfection.IsHuman)
        {
            return;
        }

        targetInfection.Infect();

        if (showDebugLogs)
        {
            Debug.Log(name + " infected " + targetInfection.name);
        }
    }

    private Vector3 GetAttackDirection(Vector2 moveDir)
    {
        Vector3 direction = new Vector3(
            moveDir.x,
            0.5f,
            moveDir.y
        );

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector3.forward;
        }

        direction.Normalize();
        return direction;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerInfection != null && playerInfection.IsInfected)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Vector3 drawDirection = attackDirection;

        if (drawDirection.sqrMagnitude < 0.001f)
        {
            drawDirection = transform.forward;
            drawDirection.y = 0f;
        }

        if (drawDirection.sqrMagnitude < 0.001f)
        {
            drawDirection = Vector3.forward;
        }

        drawDirection.Normalize();

        Vector3 drawCenter = transform.position + drawDirection * attackBoxForwardOffset;
        Quaternion drawRotation = Quaternion.LookRotation(drawDirection, Vector3.up);

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(drawCenter, drawRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize * 2f);

        Gizmos.matrix = oldMatrix;
    }
}