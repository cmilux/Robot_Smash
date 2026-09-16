using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ObjectPoolManager : NetworkBehaviour
{
    public static ObjectPoolManager instance { get; private set; }

    [Header("Player Bullet Pool")]
    [SerializeField] int playerBulletPoolSize;
    [SerializeField] PlayerBulletController playerBulletPrefab;

    [Header("Enemy Bullet Pool")]
    [SerializeField] int enemyBulletPoolSize;
    [SerializeField] TurretBullet enemyBulletPrefab;

    [Header("Kamikaze Pool")]
    [SerializeField] int kamikazePoolSize;
    [SerializeField] KamikazeEnemy kamikazePrefab;

    private Pool<PlayerBulletController> playerBulletPool;
    private Pool<TurretBullet> enemyBulletPool;
    private Pool<KamikazeEnemy> kamikazePool;
    [SerializeField] GameObject poolParentObj;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        poolParentObj.transform.SetParent(transform);

        playerBulletPool = new Pool<PlayerBulletController>(
            playerBulletPrefab,
            playerBulletPoolSize,
            poolParentObj.transform
            );

        enemyBulletPool = new Pool<TurretBullet>(
            enemyBulletPrefab,
            enemyBulletPoolSize
            );

        kamikazePool = new Pool<KamikazeEnemy>(
            kamikazePrefab,
            kamikazePoolSize);

        Debug.Log("[ObjectPoolManager] Player bullet pool initialized on server");
    }

    //player bullet get and return
    public PlayerBulletController GetPlayerBullet() => playerBulletPool.Get();
    public void ReturnPlayerBullet(PlayerBulletController playerBullet) => playerBulletPool.Return(playerBullet);

    //enemy bullet get and return
    public TurretBullet GetEnemyBullet() => enemyBulletPool.Get();
    public void ReturnEnemyBullet(TurretBullet bullet) => enemyBulletPool.Return(bullet);

    //kamikazes get and return
    public KamikazeEnemy GetKamikaze() => kamikazePool.Get();
    public void ReturnKamikaze(KamikazeEnemy kamikaze) => kamikazePool.Return(kamikaze);

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

    //return enemies
    public void ReturnEnemyAfterDelay(Enemy enemy, float delay)
    {
        StartCoroutine(ReturnEnemyAfterDelayCoroutine(enemy, delay));
    }

    private IEnumerator ReturnEnemyAfterDelayCoroutine(Enemy enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemy is KamikazeEnemy kamikaze)
        {
            ReturnKamikaze(kamikaze);
        }
    }
}
