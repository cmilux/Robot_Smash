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
    public int maxHealth;
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public float timeBeforeDestroy;

    [Header("Enemy movement")]
    protected NavMeshAgent agent;
    public float stopDistance;

    [Header("Patrol logic")]
    [SerializeField] float _patrolRadius;   //sets the radius of the patrol area for the enemy
    [SerializeField] bool _enemyWaiting;    //checks whether the enemy is waiting to go to the next patrol point or not
    protected bool _playerDetected;  //checks whether the enemy has detected the player or not
    protected bool _wasPlayerDetected;  //remembers last frame's detection state, so we can catch the exact moment the player leaves range
    public float detectionRadius = 8f;  //how close the player needs to be for this enemy to notice them and stop patrolling

    [Header("Player and experience")]
    [SerializeField] PlayerLevelUI playerLevExp;
    public int levExpPoints;
    public Transform target;            //player transform's component
    ulong killerClientId = ulong.MaxValue;               //unsigned long integer (only positive so doesnt causes compilation errors) || variable q solo almacena int positivos

    [Header("Drop resources")]
    [SerializeField] GameObject resourcesObj;
    [SerializeField] Transform spawnResPoint;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;                   //sets enemies to max health (set on inspector individually) || salud maxima de los enemigos (se pone manualmente en el inspector de cada uno)
        }
    }

    public void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");     //gets all the player active on scene || encuentra a todos los players en la escena

        float closestDistance = Mathf.Infinity;                 //largest possible value(infinity) so any real distance will be smaller || asigna el valor mas largo (inifito) para q cualquier distancia sea menor
        Transform closestPlayer = null;             //variable to find the closest player || variable para encontrar al player mas cercano

        foreach (GameObject p in players)           //for each player on players array || por cada player en el array de players
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);        //calculate distance between enemy and every player on scene || calcula la distancia entre el enemigo y los jugadores
            if (dist < closestDistance)
            {
                closestDistance = dist;             //now closest distance now is the real distance between enemy and player || ahora closestdistance tiene la distancia real entre el enemigo y el player
                closestPlayer = p.transform;        //sets the closest player transform || asigna la posicion del player mas cercanoo
            }
        }

        target = closestPlayer;         //follows closest player || sigue al player mas cercano
    }

    protected void MoveTowardTarget()
    {
        if (!IsServer) return;
        if (target == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, target.position);     //calculate distance between enemy and every player on scene || calcula la distancia entre el enemigo y los jugadores

        if (dist > stopDistance)        //if distance is > than stop distance
        {
            agent.isStopped = false;        //enemy wont stop || el enemigo no frena
            agent.SetDestination(target.position);      //follows player || persigue al player
        }
        else
        {
            agent.isStopped = true;     //stops enemy || frena al enemigo
        }
    }

    protected void HandlePatrolState()
    {
        if(!IsServer) return; 
        if (agent.pathPending || _enemyWaiting)
        {
            return;
        }

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitBeforeMoving());
        }
    }

    void UpdatePatrolPoint()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * _patrolRadius;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, _patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else return;
    }

    private IEnumerator WaitBeforeMoving()
    {
        _enemyWaiting = true;        //mark enemy as waiting so HandlePatrolState() skips it this frame
        agent.isStopped = true;      //freeze the agent while it "thinks" about where to go next

        yield return new WaitForSeconds(1);   //pause for 1 second at the patrol point before moving again

        _enemyWaiting = false;       //done waiting — clears the flag so patrol logic can run again (this was missing before, causing the enemy to freeze forever)
        agent.isStopped = false;     //let the agent move again
        UpdatePatrolPoint();         //pick a new random point to patrol toward
    }

    protected void DetectPlayer()
    {
        if (target == null)
        {
            //no target assigned yet, so nothing to detect
            _playerDetected = false;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);     //distance between enemy and its current target
        _playerDetected = dist <= detectionRadius;      //true if player is within detection range, false otherwise (triggers patrol vs chase)

        if (_wasPlayerDetected && !_playerDetected)
        {
            agent.ResetPath();
        }

        _wasPlayerDetected = _playerDetected;
    }

    protected void HandleFollowState()
    {
        if (!IsServer) return;
        agent.SetDestination(target.position);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]       //sends information to server and everyone can call this method || envia la informacion al server y cualquiera puede llamar al metodo
    public virtual void TakeDamageServerRpc(int damageAmount, ulong attackerClientId)
    {
        Debug.Log($"Hit by clientId: {attackerClientId} | Server clientId: {NetworkManager.Singleton.LocalClientId}");

        if (isDead.Value) return;

        //Takes damage from enemies
        health.Value -= damageAmount;
        killerClientId = attackerClientId;      //who killed the enemy || quien mato al enemigo

        if (health.Value <= 0)
        {
            isDead.Value = true;                                      //Enemy is dead
            Die(timeBeforeDestroy);                             //Call Die method with parameter
        }
    }

    protected virtual void Die(float delay)
    {
        if (!IsServer) return;

        //Enemy will "destroy" after some time set in parameter || el enemigo muere luego de un tiempo determinado
        StartCoroutine(DespawnAfterDelay(timeBeforeDestroy));

        DropResources();

        //Add experience to the killer || agrega experiencia a quien mato al enemigo
        GrantExpToKillerClientRpc(killerClientId, levExpPoints);
    }

    void DropResources()
    {
        if(resourcesObj == null) return;

        if (isDead.Value == true)
        {
            GameObject resources = Instantiate(resourcesObj, gameObject.transform.position, gameObject.transform.rotation);
            NetworkObject netRes = resources.GetComponent<NetworkObject>();
            netRes.Spawn();
        }
    }


    [ClientRpc]
    void GrantExpToKillerClientRpc(ulong clientId, int expAmount)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        PlayerLevelUI.Instance.AddExp(expAmount);       //add exp to player || agrega experiencia al player
    }

    IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(false);       //Despawn enemy after death || despawn enemigo una vez muerto
        }
    }

    protected virtual IEnumerator DespawnBullet(NetworkObject netObj, float timeBeforeDestroy)
    {
        yield return new WaitForSeconds(timeBeforeDestroy);

        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(false);              //Despawn bullet after some time || despawn bala desp de un tiempo
        }
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);        //turn game obj off after despawn || apaga el objeto desp de ser despawneado
        base.OnNetworkDespawn();
    }
}
