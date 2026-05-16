using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    [Header("Enemy class")]
    public int health = 3;
    public bool isDead;
    public float timeBeforeDestroy;
    public int playerExp;
    protected NavMeshAgent agent;
    public float stopDistance;

    [Header("Player experience")]
    [SerializeField] PlayerLevelUI pj;
    public Transform target;            //player transform's component

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void UpdateTarget()
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

    protected void MoveTowardTarget()
    {
        if (target == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, target.position);


        if (dist > stopDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        //Takes damage from enemies
        health -= damageAmount;

        if (health <= 0)
        {
            isDead = true;                                      //Enemy is dead
            PlayerLevelUI.Instance.AddExp(playerExp);          //Add experience to player
            Die(timeBeforeDestroy);                             //Call Die method wirh parameter
        }
    }

    protected virtual void Die(float timeBeforeDestroys)
    {
        if (!IsServer) return;
        
        //Enemy will "destroy" after some time set in parameter
        StartCoroutine(DespawnAfterDelay(timeBeforeDestroys));
    }

    IEnumerator DespawnAfterDelay(float timeBeforeDestroy)
    {
        yield return new WaitForSeconds(timeBeforeDestroy);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(false);
        }
    }

    protected virtual IEnumerator DespawnBullet(NetworkObject netObj, float timeBeforeDestroy)
    {
        yield return new WaitForSeconds(timeBeforeDestroy);

        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }
}
