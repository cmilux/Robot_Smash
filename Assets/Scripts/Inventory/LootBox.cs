using Unity.Netcode;
using UnityEngine;

public class LootBox : NetworkBehaviour
{
    public ItemData itemToDrop;

    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {   //only the serve can spawn items 
        if (!IsServer) return; 

        if (isBroken) return;

        if (!collision.gameObject.CompareTag("Player")) return;

        BreakBox();
    }

    private void BreakBox()
    {   
        isBroken = true;    

        GameObject itemDrop = Instantiate(itemToDrop.dropPrefab, transform.position, Quaternion.identity);

        NetworkObject netObj = itemDrop.GetComponent<NetworkObject>();

        netObj.Spawn();

        NetworkObject.Despawn(false);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }
}
