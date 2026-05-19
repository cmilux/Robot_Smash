using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    [Header("Enemy life")]
    public NetworkVariable<int> health = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public float timeBeforeDestroy;

    [Header("Enemy movement")]
    protected NavMeshAgent agent;
    public float stopDistance;

    [Header("Player and experience")]
    [SerializeField] PlayerLevelUI playerLevExp;
    public int levExpPoints;
    public Transform target;            //player transform's component

    ulong killerClientId;               //unsigned long integer (only positive so doesnt causes compilation errors)

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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void TakeDamageServerRpc(int damageAmount, ulong attackerClientId)
    {
        if (isDead.Value) return;

        //Takes damage from enemies
        health.Value -= damageAmount;
        killerClientId = attackerClientId;

        if (health.Value <= 0)
        {
            isDead.Value = true;                                      //Enemy is dead
            //PlayerLevelUI.Instance.AddExp(playerExp);          //Add experience to player
            Die(timeBeforeDestroy);                             //Call Die method wirh parameter
        }
    }

    protected virtual void Die(float delay)
    {
        if (!IsServer) return;

        //Enemy will "destroy" after some time set in parameter
        StartCoroutine(DespawnAfterDelay(timeBeforeDestroy));

        GrantExpToKillerClientRpc(killerClientId, levExpPoints);
    }

    [ClientRpc]
    void GrantExpToKillerClientRpc(ulong clientId, int expAmount)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId && isDead.Value == false) return;
        PlayerLevelUI.Instance.AddExp(expAmount);
    }

    IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

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
