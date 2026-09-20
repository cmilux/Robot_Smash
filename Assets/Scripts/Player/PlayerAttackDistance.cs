using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackDistance : NetworkBehaviour
{
    public Transform aim;
    [HideInInspector] public Transform[] firePoints;
    public GameObject bulletPrefab;

    public float nextFireTime;

    public float detectionRange = 25f;
    public LayerMask enemyLayer;

    public float minVerticalAngle = 10f; // How far up the gun can aim
    public float maxVerticalAngle = 15f; // How far down the gun can aim

    public float returnSpeed = 5f; // How fast the aim returns to center

    private GameObject currentEnemy;

    [SerializeField] private ItemData equippedWeaponData;//the current equipped weapon base data (damage, cooldown,etc.)

    // Network variable to share the gun rotation with all players
    private NetworkVariable<Quaternion> aimRotation =
        new NetworkVariable<Quaternion>(
            Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    private Rigidbody rb;

    // Save the original aim rotation
    private Quaternion originalAimRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsOwner)
        {
            // Find the closest enemy
            FindNearestEnemy();

            if (currentEnemy != null)
            {
                AimAtEnemy();
                TryShoot();
            }
            else
            {
                ReturnAimToCenter();
            }
        }
        else
        {
            // Update the aim rotation for other players
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
    public void SetAimAndFirePoints(Transform newAim, Transform[] newFirePoints)
    {
        aim = newAim;
        firePoints = newFirePoints;
        originalAimRotation = aim.localRotation; //recalculate resting rotation for this weapon aim
    }

    // Call by InventoryManager when the ranged weapon change
    public void SetWeaponData(ItemData weaponData)
    {
        equippedWeaponData = weaponData;
    }
    void AimAtEnemy()
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
    void ReturnAimToCenter()
    {
        // No enemy: return the aim to original rotation
        aim.localRotation = Quaternion.Slerp(
            aim.localRotation,
            originalAimRotation,
            Time.deltaTime * returnSpeed
        );

        // Update the network rotation
        if (aim.rotation != aimRotation.Value)
        {
            aimRotation.Value = aim.rotation;
        }
    }
    public void TryShoot()
    {
        if (!enabled) return;

        if (equippedWeaponData == null) return;
        if (Time.time < nextFireTime) return;

        
            Vector3 shooterVelocity = rb.linearVelocity;

            foreach(Transform point in firePoints)
            {
                // Ask the server to spawn the bullet
                ShootServerRpc(
                    point.position,
                    point.rotation,
                    rb.linearVelocity,
                    equippedWeaponData.damageBase,
                    equippedWeaponData.bulletSpeed
                );
            }
            nextFireTime = Time.time + equippedWeaponData.cooldownBase;
    }

    [ServerRpc]
    void ShootServerRpc(Vector3 pos, Quaternion rotation, Vector3 shooterVelocity, int damage, float bulletSpeed)
    {
        //pool a bullet
        PlayerBulletController bullet = ObjectPoolManager.instance.GetPlayerBullet();

        //get net transform from buller
        NetworkTransform netTransform = bullet.GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(pos, rotation, bullet.transform.localScale);
        }
        else
        {
            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
        }

        bullet.shooterClientId = OwnerClientId;
        bullet.SetDamage(damage);  
        bullet.speed = bulletSpeed;
        bullet.extraVelocity = shooterVelocity;

        bullet.gameObject.SetActive(true);

        //set velocity and other state directly on server
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;

            Vector3 bulletDirection = bullet.transform.forward;
            float forwardBoost = Vector3.Dot(shooterVelocity, bulletDirection);
            forwardBoost = Mathf.Max(forwardBoost, 0f);

            rb.linearVelocity = bulletDirection * (bulletSpeed + forwardBoost);
        }
    }
}