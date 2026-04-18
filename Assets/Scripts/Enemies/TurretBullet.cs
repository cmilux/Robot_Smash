using UnityEngine;

public class TurretBullet : Enemy
{
    public int damage = 1;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Gets the playerHealth script from player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                //Takes damage from player
                playerHealth.LoseHealth(damage);
            }

            Destroy(gameObject);        //Destroys bullet
        }
    }
}
