using UnityEngine;

public class KamikazeEnemy : Enemy
{
    public ParticleSystem _explosion;
    public Transform player;
    public float explodeDistance = 10f;
    public int damage = 1;
    public bool isDead = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if(isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= explodeDistance)
        {
            Explode();
        }
    }

    void Explode()
    {
        isDead= true;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.LoseHealth(damage);
        }

        _explosion.Play();
        timeBeforeDestroy = 5;
        Die(timeBeforeDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _explosion.Play();
            //Destroy enemy if player collides w it
            isDead = true;
            Destroy(transform.root.gameObject, 5f);
        }
    }
}
