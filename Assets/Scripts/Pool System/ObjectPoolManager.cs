using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ObjectPoolManager : NetworkBehaviour
{
    public static ObjectPoolManager instance { get; private set; }

    [Header("Player Bullet Pool")]
    [SerializeField] int playerBulletPoolSize;
    [SerializeField] PlayerBulletController playerBulletPrefab;

    private Pool<PlayerBulletController> playerBulletPool;
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

        Debug.Log("[ObjectPoolManager] Player bullet pool initialized on server");
    }

    //player bullet get and return
    public PlayerBulletController GetPlayerBullet() => playerBulletPool.Get();
    public void ReturnPlayerBullet(PlayerBulletController playerBullet) => playerBulletPool.Return(playerBullet);

}
