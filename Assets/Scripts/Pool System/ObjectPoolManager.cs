using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ObjectPoolManager : NetworkBehaviour
{
    public static ObjectPoolManager instance { get; private set; }

    [Header("Player Bullet Pool")]
    [SerializeField] int playerBulletPoolSize;
    [SerializeField] PlayerBulletController playerBulletPrefab;

    [Header("Enemy Bullet Pool")]
    [SerializeField] int enemyBulletPoolSize;
    [SerializeField] TurretBullet enemyBulletPrefab;

    [Header("Spawm Kamikaze Pool")]
    [SerializeField] int spawnKamikazePoolSize;
    [SerializeField] KamikazeEnemy spawnKamikazePrefab;

    [Header("Kamikaze Pool")]
    [SerializeField] int kamikazePoolSize;
    [SerializeField] KamikazeEnemy kamikazePrefab;
    [SerializeField] KamikazeEnemy kamikazeMediumPrefab;
    [SerializeField] KamikazeEnemy kamikazeHardPrefab;

    [Header("Turret Pool")]
    [SerializeField] int turretPoolSize;
    [SerializeField] TurretEnemy turretPrefab;
    [SerializeField] TurretEnemy turretMediumPrefab;
    [SerializeField] TurretEnemy turretHardPrefab;

    [Header("Big Enemy Pool")]
    [SerializeField] int bigEnemyPoolSize;
    [SerializeField] BigEnemy bigPrefab;
    [SerializeField] BigEnemy bigMediumPrefab;
    [SerializeField] BigEnemy bigHardPrefab;

    //bullets || municion
    private Pool<PlayerBulletController> playerBulletPool;
    private Pool<TurretBullet> enemyBulletPool;
    private Pool<KamikazeEnemy> spawnKamikazePool;

    private Pool<KamikazeEnemy> kamikazePool, kamikazeMediumPool, kamikazeHardPool;
    private Pool<TurretEnemy> turretPool, turretMediumPool, turretHardPool;
    private Pool<BigEnemy> bigPool, bigMediumPool, bigHardPool;

    [SerializeField] GameObject poolParentObj;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        //Find a valid navmesh position from pool spawning
        Vector3 poolSpawnPos = Vector3.zero;
        if (NavMesh.SamplePosition(Vector3.zero, out NavMeshHit hit, 50f, NavMesh.AllAreas))
        {
            poolSpawnPos = hit.position;
        }

        //create pool parent at valid position
        poolParentObj = new GameObject("===PooledObj===");
        poolParentObj.transform.position = poolSpawnPos;

        //poolParentObj.transform.SetParent(transform);

        playerBulletPool = new Pool<PlayerBulletController>(
            playerBulletPrefab,
            playerBulletPoolSize,
            poolParentObj.transform
            );

        enemyBulletPool = new Pool<TurretBullet>(
            enemyBulletPrefab,
            enemyBulletPoolSize
            );

        spawnKamikazePool = new Pool<KamikazeEnemy>(
            spawnKamikazePrefab,
            spawnKamikazePoolSize);

        // Kamikaze pools
        kamikazePool = new Pool<KamikazeEnemy>(kamikazePrefab, kamikazePoolSize);
        kamikazeMediumPool = new Pool<KamikazeEnemy>(kamikazeMediumPrefab, kamikazePoolSize);
        kamikazeHardPool = new Pool<KamikazeEnemy>(kamikazeHardPrefab, kamikazePoolSize);

        // Turret pools
        turretPool = new Pool<TurretEnemy>(turretPrefab, turretPoolSize);
        turretMediumPool = new Pool<TurretEnemy>(turretMediumPrefab, turretPoolSize);
        turretHardPool = new Pool<TurretEnemy>(turretHardPrefab, turretPoolSize);

        // Big Enemy pools
        bigPool = new Pool<BigEnemy>(bigPrefab, bigEnemyPoolSize);
        bigMediumPool = new Pool<BigEnemy>(bigMediumPrefab, bigEnemyPoolSize);
        bigHardPool = new Pool<BigEnemy>(bigHardPrefab, bigEnemyPoolSize);

    }

    //player bullet get and return
    public PlayerBulletController GetPlayerBullet() => playerBulletPool.Get();
    public void ReturnPlayerBullet(PlayerBulletController playerBullet) => playerBulletPool.Return(playerBullet);

    //enemy bullet get and return
    public TurretBullet GetEnemyBullet() => enemyBulletPool.Get();
    public void ReturnEnemyBullet(TurretBullet bullet) => enemyBulletPool.Return(bullet);

    //kamikazes get and return
    public KamikazeEnemy GetKamikaze() => spawnKamikazePool.Get();
    public void ReturnKamikaze(KamikazeEnemy kamikaze) => spawnKamikazePool.Return(kamikaze);

    //return enemies bullets
    public void ReturnEnemyBulletAfterDelay(TurretBullet bullet, float delay)
    {
        StartCoroutine(ReturnEnemyBulletDelayCoroutine(bullet, delay));
    }

    private IEnumerator ReturnEnemyBulletDelayCoroutine(TurretBullet bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnEnemyBullet(bullet);
    }

    //get enemies
    public Enemy GetEnemy(Enemy prefab)
    {
        if (prefab == null) return null;

        string prefabName = prefab.name.ToLower();

        if (prefab is KamikazeEnemy kam)
        {
            if (prefab.name.Contains("Medium")) return kamikazeMediumPool.Get();
            if (prefab.name.Contains("Hard")) return kamikazeHardPool.Get();
            return kamikazePool.Get();
        }
        else if (prefab is TurretEnemy turr)
        {
            if (prefab.name.Contains("Medium")) return turretMediumPool.Get();
            if (prefab.name.Contains("Hard")) return turretHardPool.Get();
            return turretPool.Get();
        }
        else if (prefab is BigEnemy big)
        {
            if (prefab.name.Contains("Medium")) return bigMediumPool.Get();
            if (prefab.name.Contains("Hard")) return bigHardPool.Get();
            return bigPool.Get();
        }

        return null;
    }

    //return enemies
    private IEnumerator ReturnEnemyAfterDelayCoroutine(Enemy enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemy is KamikazeEnemy kam)
        {
            if (kam.name.Contains("Medium")) kamikazeMediumPool.Return(kam);
            else if (kam.name.Contains("Hard")) kamikazeHardPool.Return(kam);
            else kamikazePool.Return(kam);
        }
        else if (enemy is TurretEnemy tur)
        {
            if (tur.name.Contains("Medium")) turretMediumPool.Return(tur);
            else if (tur.name.Contains("Hard")) turretHardPool.Return(tur);
            else turretPool.Return(tur);
        }
        else if (enemy is BigEnemy big)
        {
            if(big.name.Contains("Medium")) bigMediumPool.Return(big);
            else if(big.name.Contains("Hard")) bigHardPool.Return(big);
            else bigPool.Return(big);
        }
    }

    public void ReturnEnemyAfterDelay(Enemy enemy, float delay)
    {
        StartCoroutine(ReturnEnemyAfterDelayCoroutine(enemy, delay));
    }
}
