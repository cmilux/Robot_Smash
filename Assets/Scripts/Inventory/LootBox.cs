using Unity.Netcode;
using UnityEngine;

public class LootBox : NetworkBehaviour
{
    //Template used to set items and amount in the Inspector
    [System.Serializable]
    public class LootOption
    {
        public ItemData item;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    //All the items this box could drop (set in the inspector)
    public LootOption[] lootOptions;

    //How many hits the box needs before breaking
    public int hitsToBreak = 3;
    private int hitsTaken = 0;

    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (isBroken) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        OnHit();
    }

    // Called every time the box gets hit//FIX!
    private void OnHit()
    {
        hitsTaken++;

        if (hitsTaken >= hitsToBreak)
        {
            Break();
        }
    }

    //Breaks the box and drops one random item
    private void Break()
    {
        isBroken = true;

        //Pick one random option from the list
        LootOption chosen = lootOptions[Random.Range(0, lootOptions.Length)];

        //Pick a random amount between the min and max
        int amount = Random.Range(chosen.minAmount, chosen.maxAmount + 1);

        GameObject droppedItem = Instantiate(chosen.item.dropPrefab, transform.position, Quaternion.identity);

        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.quantity = amount;
        }

        NetworkObject netObj = droppedItem.GetComponent<NetworkObject>();
        netObj.Spawn();

        QuestManager.Instance.ReportProgress(ObjectiveType.DestroyObject, chosen.item.id.ToString());

        NetworkObject.Despawn(false);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }
}