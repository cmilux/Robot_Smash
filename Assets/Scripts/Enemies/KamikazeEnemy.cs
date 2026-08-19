using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class KamikazeEnemy : Enemy
{
    [Header("Explosion Attack")]
    public ParticleSystem _explosion;       //explosion particles
    public float explodeDistance = 10f;     //distance to explode
    public int damage = 1;                  //amount of damage caused by enemy
    bool kamIsDead = false;                 //bool to check if enemy is dead
    bool methodRun = false;                 //bool to check if distance method has run

    protected override void Start()
    {
        base.Start();
        UpdateTarget();
    }

    private void Update()
    {
        if (!IsServer) return;         //server-only — clients don't run enemy AI logic, they just see the result
        if (isDead.Value) return;      //dead enemies don't act

        UpdateTarget();                 //re-check who the closest player is every frame
        if (target == null) return;    //no players in scene, nothing to do

        DetectPlayer();                 //checks distance to target and sets _playerDetected accordingly

        if (!_playerDetected)
        {
            HandlePatrolState();        //player is out of range — keep wandering patrol points
        }
        else
        {
            MoveTowardTarget();         //player is within detectionRadius — chase them directly (stopDistance can be ~0 so it walks into contact range for the explosion collision)
        }
    }

    //need to find a way to cancel the attack when player moves away
    void Distance()
    {
        if (isDead.Value || target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= explodeDistance && !methodRun)
        {
            methodRun = true;
            Explode();
        }
    }

    void Explode()
    {
        if (isDead.Value) return;

        PlayExplosionClientRpc();
        agent.isStopped = true;     //Enemy stops
        timeBeforeDestroy = 5;      //Set time to be destroyed
        Die(timeBeforeDestroy);     //Enemy death method is called
        kamIsDead = true;           //Enemy is dead now

        if (target != null)
        {
            //Get the player health script
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                //Applies damage to player
                playerHealth.LoseHealthServerRpc(damage);
            }
        }
    }

    [ClientRpc]
    void PlayExplosionClientRpc()
    {
        _explosion.Play();          //Play particles on clients scene too
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (isDead.Value) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            target = collision.transform;
            Explode();
        }
    }
}
