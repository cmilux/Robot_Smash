using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackDistance : MonoBehaviour
{
    public Transform aim;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float detectionRange = 25f;
    public LayerMask enemyLayer;

    private GameObject currentEnemy;

    void Update()
    {
        //look for the closest enemy to keep the aim updated(esto puede traer problemas por buscar en cada frame, podria utilizar invoke para hacerlo cada determinados segundos en start)
        FindNearestEnemy();

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
            }
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
        if (value.isPressed)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}