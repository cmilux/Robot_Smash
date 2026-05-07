using UnityEngine;
using UnityEngine.AI;

public class KamikazeEnemy : Enemy
{
    NavMeshAgent agent;
    public ParticleSystem _explosion;       //explosion particles
    public Transform target;                //player transform's component
    public float explodeDistance = 10f;     //distance to explode
    public int damage = 1;                  //amount of damage caused by enemy
    bool kamIsDead = false;                 //bool to check if enemy is dead
    bool methodRun = false;                 //bool to check if distance method has run

    private void Start()
    {
        //Gets the nav mesh
        agent = GetComponent<NavMeshAgent>();

        UpdateTarget();
    }

    private void Update()
    {
        if (kamIsDead) return;

        UpdateTarget(); // refresh closest player every frame

        if (target == null) return;

        agent.SetDestination(target.position);
        //Distance();
    }

    void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = p.transform;
            }
        }

        target = closestPlayer;
    }


    //need to find a way to cancel the attack when player moves away
    void Distance()
    {
        if (kamIsDead || target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= explodeDistance && !methodRun)
        {
            methodRun = true;
            Explode();
        }
    }

    void Explode()
    {
        if (kamIsDead) return;

        _explosion.Play();
        agent.isStopped = true;
        timeBeforeDestroy = 5;      //Set time to be destroyed
        Die(timeBeforeDestroy);
        kamIsDead = true;           //Enemy is dead now

        if (target != null)
        {
            //Get the player health script
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
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
            target = collision.transform;
            Explode();
        }
    }
}
