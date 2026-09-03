using System.Collections;
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

    // How long to wait after breaking before the item appears
    public float dropDelay = 2f;

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

    //Marks the box as broken and start the coroutine
    private void Break()
    {
        isBroken = true;

        StartCoroutine(DropAfterDelay());
    }

    //Wait a few seconds and then spawn the item
    private IEnumerator DropAfterDelay()
    {   
        yield return new WaitForSeconds(dropDelay);

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