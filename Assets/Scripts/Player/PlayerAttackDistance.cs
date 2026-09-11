using Unity.Netcode;
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

    public int currentAmmo; //how much ammo the player currently has left

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
            else
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

        //refill ammo when a new weapon is equipped (only if it uses ammo)
        if(weaponData != null && weaponData.maxAmmo >= 0)
        {
            currentAmmo = weaponData.maxAmmo;
        }

    }
    public void OnAttack(InputValue value)
    {
        if (!enabled) return;

        if (!IsOwner) return;

        if (equippedWeaponData == null) return;

        // Block fire if this weapon use ammo and there is none left
        if (equippedWeaponData.maxAmmo >= 0 && currentAmmo <= 0) return;

        if (value.isPressed && Time.time >= nextFireTime)
        {
            Vector3 shooterVelocity = rb.linearVelocity;

            foreach(Transform point in firePoints)
            {
                // Ask the server to spawn the bullet
                ShootServerRpc(
                    point.position,
                    point.rotation,
                    shooterVelocity,
                    equippedWeaponData.damageBase, equippedWeaponData.bulletSpeed
                );
            }


            //only consume ammo if this weapon actually use it
            if (equippedWeaponData.maxAmmo >= 0)
            {
                currentAmmo--;
            }
            nextFireTime = Time.time + equippedWeaponData.cooldownBase;
        }
    }

    // The server creates the bullet
    // and gives ownership to the player who shot it
    [ServerRpc]
    void ShootServerRpc(
        Vector3 position,
        Quaternion rotation,
        Vector3 shooterVelocity, int damage, float bulletSpeed)
    {
        GameObject bullet =
            Instantiate(bulletPrefab, position, rotation);

        PlayerBulletController bulletController =
            bullet.GetComponent<PlayerBulletController>();

        if (bulletController != null)
        {
            bulletController.extraVelocity = shooterVelocity;
            bulletController.damage = damage;
            bulletController.speed = bulletSpeed;
        }

        bullet.GetComponent<NetworkObject>()
            .SpawnWithOwnership(OwnerClientId);
    }
}