using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponControllerAutoAim : MonoBehaviour
{
    public Transform aim;
    public Transform firePoint;
    public GameObject bulletPrefab;
    void Update()
    {
        // Get all enemies in the scene and add to the array enemies 
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        //If there are no enemies exit
        if (enemies.Length == 0) return;

        //Pick a random enemy from the array
        GameObject enemy = enemies[Random.Range(0, enemies.Length)];

        //calculate direction towards the enemy
        Vector3 direction = enemy.transform.position - aim.position;
        direction.y = 0;

        //Rotate the aim towards the enemy (esto no esta funcionando bien, y no se soluciona poniendo un tiempo por enemigo, podria ser con un rango)
        aim.LookAt(aim.position + direction);
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}