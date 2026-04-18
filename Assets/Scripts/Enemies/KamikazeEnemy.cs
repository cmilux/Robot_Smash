using UnityEngine;

public class KamikazeEnemy : Enemy
{
    public ParticleSystem _explosion;
    public Transform player;
    public float explodeDistance = 10f;
    public int damage = 1;
    bool kamIsDead = false;

    private void Start()
    {
        //Get the player transform component
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        //Distance();
    }


    //There is an issue w the distance attack, it kills the player at once
    void Distance()
    {
        if (kamIsDead) return;

        //Calculates the distance between player an enemy
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= explodeDistance)
        {
            Explode();

            if (distance <= explodeDistance)
            {
                //Get the player health script
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    //Applies damage to player
                    playerHealth.LoseHealth(damage);
                }
            }
        }
    }

    void Explode()
    {
        if (kamIsDead) return;

        _explosion.Play();
        timeBeforeDestroy = 5;      //Set time to be destroyed
        Die(timeBeforeDestroy);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (kamIsDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            //Get the player health script
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                //Applies damage to player
                playerHealth.LoseHealth(damage);
            }

            _explosion.Play();
            //Destroy enemy if player collides w it
            kamIsDead = true;
            //Destroy(transform.root.gameObject, 5f);
        }
    }
}
