using UnityEngine;

public class TurretBullet : Enemy
{
    public int damage = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.LoseHealth(damage);
            }
            Destroy(gameObject);
        }
    }
}
