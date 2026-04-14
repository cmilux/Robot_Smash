using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3; 

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            Destroy(gameObject); 
        }
    }
}
