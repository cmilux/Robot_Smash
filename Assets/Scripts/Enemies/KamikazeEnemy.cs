using UnityEngine;

public class KamikazeEnemy : Enemy
{
    public ParticleSystem _explosion;       //explosion particles
    public Transform player;                //player transform's component
    public float explodeDistance = 10f;     //distance to explode
    public int damage = 1;                  //amount of damage caused by enemy
    bool kamIsDead = false;                 //bool to check if enemy is dead
    bool methodRun = false;                 //bool to check if distance method has run

    private void Start()
    {
        //Get the player transform component
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        //Distance();
    }


    //need to find a way to cancel the attack when player moves away
    void Distance()
    {
        if (kamIsDead) return;

        //Calculates the distance between player an enemy
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= explodeDistance && !methodRun)
        {
            methodRun = true;

            Explode();

            //Get the player health script
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                //Applies damage to player
                playerHealth.LoseHealth(damage);
                Debug.Log("Player damage");
            }
        }
    }

    void Explode()
    {
        if (kamIsDead) return;

        _explosion.Play();
        timeBeforeDestroy = 5;      //Set time to be destroyed
        Die(timeBeforeDestroy);
        kamIsDead = true;           //Enemy is dead now

        if (!methodRun)
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

    private void OnCollisionEnter(Collision collision)
    {
        if (kamIsDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
        }
    }
}
