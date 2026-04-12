using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponControllerAutoAim : MonoBehaviour
{
    public Transform aim;
    public Transform firePoint;
    public GameObject bulletPrefab;

    private GameObject currentEnemy; 

    void Update()
    {
        if (currentEnemy == null)
        {
            // Get all enemies in the scene and add to the array enemies 
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            // If there are no enemies exit
            if (enemies.Length == 0) return;

            // Pick a random enemy from the array
            currentEnemy = enemies[Random.Range(0, enemies.Length)];
        }
        if (currentEnemy != null)
        {
            // calculate direction towards the enemy
            Vector3 direction = currentEnemy.transform.position - aim.position;
            direction.y = 0;

            // Rotate the aim towards the enemy
            aim.LookAt(aim.position + direction);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}