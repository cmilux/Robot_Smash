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
        if(!IsServer) return;
        if (isDead.Value) return;

        UpdateTarget(); // refresh closest player every frame || calcula el player mas cercano

        if (target == null) return;

        MoveTowardTarget(); //follow player || persigue al player
        //Distance();
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
