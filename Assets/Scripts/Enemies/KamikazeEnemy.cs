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

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= explodeDistance)
        {
            Explode();

            if (distance <= explodeDistance)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.LoseHealth(damage);
                }
            }
        }
    }

    void Explode()
    {
        if (kamIsDead) return;

        _explosion.Play();
        timeBeforeDestroy = 5;
        Die(timeBeforeDestroy);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (kamIsDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.LoseHealth(damage);
            }

            _explosion.Play();
            //Destroy enemy if player collides w it
            kamIsDead = true;
            Destroy(transform.root.gameObject, 5f);
        }
    }
}
