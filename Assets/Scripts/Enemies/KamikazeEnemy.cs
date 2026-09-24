using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class KamikazeEnemy : Enemy
{
    [Header("Explosion Attack")]
    public ParticleSystem _explosion;       //explosion particles
    public float explodeDistance = 10f;     //distance to explode
    public int damage = 1;                  //amount of damage caused by enemy

    protected override void Start()
    {
        base.Start();
        UpdateTarget();
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    private void Update()
    {
        //Debug.Log($"[Kamikaze] Update. IsServer={IsServer}, isDead={isDead.Value}, target={target}");

        if (!IsServer) return;         //server-only — clients don't run enemy AI logic, they just see the result
        if (isDead.Value) return;      //dead enemies don't act

        UpdateTarget();                 //re-check who the closest player is every frame
        if (target == null)
        {
            //Debug.Log("[Kamikaze] No target, doing patrol");
            return;
        }

        DetectPlayer();                 //checks distance to target and sets _playerDetected accordingly

        if (!_playerDetected)
        {
            //Debug.Log("[Kamikaze] Player not detected, patrol");
            HandlePatrolState();        //player is out of range — keep wandering patrol points

            if (_enemyWaiting)
            {
                animator.SetBool("IsStopped", true);
                animator.SetBool("IsPlayerDetected", false);
            }
            else
            {
                animator.SetBool("IsStopped", false); 
                animator.SetBool("IsPlayerDetected", false);
            }
        }
        else
        {
            MoveTowardTarget();         //player is within detectionRadius — chase them directly (stopDistance can be ~0 so it walks into contact range for the explosion collision)
            animator.SetBool("IsPlayerDetected", true);
        }
    }

    void Explode()
    {
        if (isDead.Value) return;

        isDead.Value = true;
        agent.isStopped = true;     //Enemy stops
        PlayExplosionClientRpc();

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

        Die(timeBeforeDestroy);     //Enemy death method is called
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
