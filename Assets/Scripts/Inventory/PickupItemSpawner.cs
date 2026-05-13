using Unity.Netcode;
using UnityEngine;
public class PickupItemSpawner : NetworkBehaviour
{
    public GameObject[] pickupPrefabs;
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject pickup = Instantiate(
                pickupPrefabs[i],
                spawnPoints[i].position,
                spawnPoints[i].rotation);

            pickup.GetComponent<NetworkObject>().Spawn();
        }
    }
}
