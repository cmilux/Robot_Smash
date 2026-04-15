using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.LoseHealth(damage);
        }
    }
}
