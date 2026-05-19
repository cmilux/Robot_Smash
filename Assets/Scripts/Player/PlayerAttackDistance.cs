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
  

    private GameObject currentEnemy;
    private NetworkVariable<Quaternion> aimRotation = 
        new NetworkVariable<Quaternion>(Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

    void Update()
    {
       if (IsOwner)
        {
            //look for the closest enemy to keep the aim updated(esto puede traer problemas por buscar en cada frame, podria utilizar invoke para hacerlo cada determinados segundos en start)
            FindNearestEnemy();
            //InvokeRepeating(nameof(FindNearestEnemy), 0f, 0.2f); podria hacer esto para optimizar

            if (currentEnemy != null)
            {
                // Calculate direction towards the enemy
                Vector3 direction = currentEnemy.transform.position - aim.position;
                //direction.y = 0;

                // Rotate the aim towards the enemy
                if (direction != Vector3.zero)
                {
                    //aim.LookAt(aim.position + direction);
                    aim.LookAt(currentEnemy.transform);

                    // Solo actualiza si cambio la rotacion
                    if (aim.rotation != aimRotation.Value)
                    {
                        aimRotation.Value = aim.rotation;
                    }
                }
            }
        }
        else
        {
            aim.rotation = aimRotation.Value;
        }

    }

    void FindNearestEnemy()
    {
        //Get all enemies in the range
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        GameObject closest = null;

        float minDistance = detectionRange;

        // for each collider first verifies if its an enemy
        foreach (Collider col in collidersInRange)
        {
            // Calculate the distance to each enemy
            float distance = Vector3.Distance(transform.position, col.transform.position);

            //Check if this enemy is the closest
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
        if (!enabled) return;

        if (!IsOwner) return;


        if (value.isPressed && Time.time >= nextFireTime)
        {
            ShootServerRpc(firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + cooldownShoot;
        }
    }
    [ServerRpc]
    void ShootServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
    }
}