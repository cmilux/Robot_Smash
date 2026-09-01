using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackDistance : NetworkBehaviour
{
    public Transform aim;
    public Transform firePoint;
    public GameObject bulletPrefab;

    public float cooldownShoot = 1;
    public float nextFireTime;

    public float detectionRange = 25f;
    public LayerMask enemyLayer;

    public float minVerticalAngle = 10f; // How far up the gun can aim
    public float maxVerticalAngle = 15f; // How far down the gun can aim

    private GameObject currentEnemy;

    // Network variable to share the gun rotation with all players
    private NetworkVariable<Quaternion> aimRotation =
        new NetworkVariable<Quaternion>(
            Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    void Update()
    {
        if (IsOwner)
        {
            // Find the closest enemy
            FindNearestEnemy();

            // Aim at the current enemy
            if (currentEnemy != null)
            {
                // Calculate direction towards the enemy
                Vector3 direction = currentEnemy.transform.position - aim.position;

                if (direction != Vector3.zero)
                {
                    // Rotate the aim towards the enemy
                    aim.LookAt(aim.position + direction);

                    // Get the current local rotation
                    Vector3 angles = aim.localEulerAngles;

                    // Convert X from 0-360 to -180 to 180
                    float verticalAngle = angles.x;

                    if (verticalAngle > 180f)
                    {
                        verticalAngle -= 360f;
                    }

                    // Limit the X rotation
                    // Negative values = up
                    // Positive values = down
                    verticalAngle = Mathf.Clamp(
                        verticalAngle,
                        -minVerticalAngle,
                        maxVerticalAngle
                    );

                    // Apply the limited X rotation
                    angles.x = verticalAngle;
                    aim.localEulerAngles = angles;

                    // Update the network rotation if it changed
                    if (aim.rotation != aimRotation.Value)
                    {
                        aimRotation.Value = aim.rotation;
                    }
                }
            }
        }
        else
        {
            // Copy the rotation from the owner
            aim.rotation = aimRotation.Value;
        }
    }
    void FindNearestEnemy()
    {
        // Get all enemies inside the detection range
        Collider[] collidersInRange =
            Physics.OverlapSphere(
                transform.position,
                detectionRange,
                enemyLayer
            );

        GameObject closest = null;

        float minDistance = detectionRange;

        // Check every enemy found
        foreach (Collider col in collidersInRange)
        {
            // Calculate the distance to the enemy
            float distance =
                Vector3.Distance(transform.position, col.transform.position);

            // Check if this enemy is closer
            if (distance < minDistance)
            {
                closest = col.gameObject;
                minDistance = distance;
            }
        }

        currentEnemy = closest;
    }
    public void OnAttack(InputValue value)
    {
        if (!enabled)
            return;

        if (!IsOwner)
            return;

        if (value.isPressed && Time.time >= nextFireTime)
        {
            // Ask the server to spawn the bullet
            ShootServerRpc(
                firePoint.position,
                firePoint.rotation
            );

            nextFireTime = Time.time + cooldownShoot;
        }
    }

    // The server creates the bullet
    // and gives ownership to the player who shot it
    [ServerRpc]
    void ShootServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject bullet =
            Instantiate(
                bulletPrefab,
                position,
                rotation
            );

        bullet.GetComponent<NetworkObject>()
            .SpawnWithOwnership(OwnerClientId);
    }
}