using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    public float aimPlayerErrorRadius = 0.75f;
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
    [SerializeField] protected Image _enemyHealthFill;

    [Header("Enemy movement")]
    protected NavMeshAgent agent;
    public float stopDistance;
    [SerializeField] float _turnSpeed = 180f;   //degrees per second
    Vector3 _spawnPoint;

    [Header("Patrol logic")]
    [SerializeField] float _patrolRadius;   //sets the radius of the patrol area for the enemy
    [SerializeField] protected bool _enemyWaiting;    //checks whether the enemy is waiting to go to the next patrol point or not
    protected bool _playerDetected;  //checks whether the enemy has detected the player or not
    protected bool _wasPlayerDetected;  //remembers last frame's detection state, so we can catch the exact moment the player leaves range
    public float detectionRadius = 8f;  //how close the player needs to be for this enemy to notice them and stop patrolling

    [Header("Animations")]
    [SerializeField] protected Animator animator;

    [Header("Explosion Attack")]
    public ParticleSystem _explosion;       //explosion particles

    [Header("Quests and pools")]
    [SerializeField] QuestArea _questArea;
    private NetworkVariable<bool> _poolActive = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

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
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChanged;

        if (IsServer)
        {
            health.Value = maxHealth;                   //sets enemies to max health (set on inspector individually) || salud maxima de los enemigos (se pone manualmente en el inspector de cada uno)
            isDead.Value = false;
        }

        UpdateEnemyHealth(health.Value, maxHealth);

        _poolActive.OnValueChanged += OnPoolActiveChanged;
        if (!IsServer) gameObject.SetActive(_poolActive.Value); // late joiners read the current value
    }

    public virtual void Initialize()
    {
        _playerDetected = false;
        _wasPlayerDetected = false;

        if (IsServer)
        {
            health.Value = maxHealth;
            isDead.Value = false;
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.enabled = true;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    [ClientRpc]
    public void NotifyActivateClientRpc()
    {
        if (IsServer) return;
        gameObject.SetActive(true);
    }

    #region SpawnPoints
    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        _spawnPoint = newSpawnPoint;
    }

    public Vector3 GetSpawnPoint()
    {
        return _spawnPoint;
    }
    #endregion

    public void UpdateTarget()
    {
        float closestSqrDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (PlayerHealth p in PlayerHealth.All)
        {
            if (p == null || p.isDead.Value) continue;  //ignore dead players || ignora a los jugadores muertos

            //square distance
            float sqrDist = (transform.position - p.transform.position).sqrMagnitude;

            //keep the closest player so far || se queda con el jugador mas cercano hasta el momento
            if (sqrDist < closestSqrDistance)
            {
                closestSqrDistance = sqrDist;
                closestPlayer = p.transform;
            }
        }

        target = closestPlayer;         //follows closest alive player || sigue al player vivo mas cercano
    }

    protected void UpdateAnimator()
    {
        if (agent == null) return;
        animator?.SetFloat("Speed", agent.velocity.magnitude);
    }

    [ClientRpc]
    public void PlayExplosionClientRpc()
    {
        AudioManager.Instance?.PlaySfxAt(SFXId.Explosion, transform.position);

        _explosion.Play();          //Play particles on clients scene too
    }

    #region Patrol
    protected void MoveTowardTarget()
    {
        if (!IsServer) return;
        if (target == null || agent == null || health.Value <= 0) return;

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

    protected void RotateTowardsTarget()
    {
        if(target == null) return;

        //flat directiom, only turn left/right, never tild
        //direccion plana, solo gira izq/der, no se inclina
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;

        //turn toward the target at a fixed speed || gira hacia el objetivo a velocidad fija
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, _turnSpeed * Time.deltaTime);
    }

    protected void HandlePatrolState()
    {
        if (!IsServer) return;
        if (agent.pathPending || _enemyWaiting || health.Value <= 0)
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

    public virtual void HandleFollowState()
    {
        if (!IsServer) return;
        agent.SetDestination(target.position);
    }
#endregion
    public void SetQuestArea(QuestArea area)
    {
        _questArea = area;
    }

    #region Sync Pools
    // server only: tells clients to show or hide this enemy
    public void SyncActiveState(bool value)
    {
        if(IsServer) _poolActive.Value = value;
    }

    void OnPoolActiveChanged(bool prev, bool next)
    {
        if (!IsServer) gameObject.SetActive(next);
    }

    #endregion

    #region EnemyHealth
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]       //sends information to server and everyone can call this method || envia la informacion al server y cualquiera puede llamar al metodo
    public virtual void TakeDamageServerRpc(int damageAmount, ulong attackerClientId)
    {
        if (isDead.Value) return;

        //Takes damage from enemies
        health.Value -= damageAmount;
        int currentHealth = health.Value;
        killerClientId = attackerClientId;      //who killed the enemy || quien mato al enemigo

        if (health.Value <= 0)
        {
            isDead.Value = true;                                      //Enemy is dead

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            Die(timeBeforeDestroy);                             //Call Die method with parameter
        }
    }

    void OnHealthChanged(int prevValue, int newValue)
    {
        UpdateEnemyHealth(newValue, maxHealth);
    }

    void UpdateEnemyHealth(int current, int max)
    {
        if (_enemyHealthFill != null) _enemyHealthFill.fillAmount = (float)current / max;
    }

    protected virtual void Die(float delay)
    {
        if (!IsServer) return;

        agent.isStopped = true;
        agent.GetComponentInChildren<Animator>().enabled = false;

        if (isDead.Value)
        {
            _questArea?.OnEnemyDied(this);

            PlayExplosionClientRpc();

            ObjectPoolManager.instance.ReturnEnemyAfterDelay(this, timeBeforeDestroy);

            //Add experience to the killer || agrega experiencia a quien mato al enemigo
            GrantExpToKillerClientRpc(killerClientId, levExpPoints);

            //reports using the enemy's tag as targetId
            QuestManager.Instance.ReportProgress(ObjectiveType.KillEnemy, gameObject.tag);
        }    
    }

    #endregion

    void DropResources()
    {
        if (resourcesObj == null) return;

        if (isDead.Value == true)
        {
            GameObject resources = Instantiate(resourcesObj, gameObject.transform.position, gameObject.transform.rotation);
            NetworkObject netRes = resources.GetComponent<NetworkObject>();
            netRes.Spawn();
        }
    }

    //let the clients know the game obj is off
    [ClientRpc]
    public void NotifyDespawnClientRpc()
    {
        if (IsServer) return;
        gameObject.SetActive(false);
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
        health.OnValueChanged -= OnHealthChanged;
        _poolActive.OnValueChanged -= OnPoolActiveChanged;

        gameObject.SetActive(false);        //turn game obj off after despawn || apaga el objeto desp de ser despawneado
        base.OnNetworkDespawn();
    }
}
