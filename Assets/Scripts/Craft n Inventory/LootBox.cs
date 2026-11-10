using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

    // How long to wait after breaking before the item appears
    public float dropDelay = 2f;

    [Header("Respawn")]
    public bool canRespawn = true;
    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false);
    [SerializeField] GameObject _model;
    [SerializeField] GameObject _brokenModel;
    public float respawnTimeMin = 40f;
    public float respawnTimeMax = 80f;
    private GameObject _droppedItem;

    //every player who hit this box, so each one gets credit for the tutorial step
    HashSet<ulong> _hitters = new HashSet<ulong>();

    [Header("Item Spawn Height")]
    [SerializeField] float _itemHeight = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (isBroken.Value) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        //remember who hit it (the collider might be a child, so look in parents)
        NetworkObject playerNetObj = collision.gameObject.GetComponentInParent<NetworkObject>();
        if (playerNetObj != null) _hitters.Add(playerNetObj.OwnerClientId);

        Break();
    }

    //Marks the box as broken and start the coroutine
    private void Break()
    {
        isBroken.Value = true;

        //tell every player who hit the box that it broke
        foreach (ulong clientId in _hitters)
        {
            NotifyBoxBrokenClientRpc(clientId);
        }

        StartCoroutine(DropAfterDelay());
    }
    private void ApplyBrokenState(bool broken)
    {
        _model.SetActive(!broken);
        if (!broken) _brokenModel.SetActive(false);
        gameObject.GetComponent<Collider>().enabled = !broken;
    }
    private void OnBrokenChanged(bool oldValue, bool newValue)
    {
        ApplyBrokenState(newValue);
        _brokenModel.SetActive(newValue);
    }
    //Wait a few seconds and then spawn the item
    private IEnumerator DropAfterDelay()
    {   
        yield return new WaitForSeconds(dropDelay);

        //Pick one random option from the list
        LootOption chosen = lootOptions[Random.Range(0, lootOptions.Length)];

        //Pick a random amount between the min and max
        int amount = Random.Range(chosen.minAmount, chosen.maxAmount + 1);

        //find the ground below the lot
        Vector3 spawnPos = transform.position;

        if (Physics.Raycast(
            transform.position + Vector3.up * 2f,
            Vector3.down,
            out RaycastHit hit,
            10f))
        {
            //spawn the item slightly above the ground
            spawnPos = hit.point + Vector3.up * _itemHeight;
        }

        //spawn the item 
        GameObject droppedItem = Instantiate(
            chosen.item.dropPrefab,
            spawnPos, 
            Quaternion.identity
            );
        _droppedItem = droppedItem;
        NetworkObject netObj = droppedItem.GetComponent<NetworkObject>();
        netObj.Spawn();
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        
        if (pickup != null)
        {
            pickup.Initialize(amount, -1);
        }

        QuestManager.Instance.ReportProgress(
            ObjectiveType.DestroyObject,
            chosen.item.id.ToString()
            );
        if (!canRespawn)
        {
            NetworkObject.Despawn(false);
        }
        else
        {
            StartCoroutine(RespawnAfterDelay(Random.Range(respawnTimeMin, respawnTimeMax)));
        }

    }
    private bool IsSpaceOccupied()
    {
        if (_droppedItem != null && _droppedItem.activeInHierarchy) return true;

        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 center = transform.TransformPoint(box.center);
        Vector3 halfSize = box.size * 0.5f;
        Collider[] collidersInside = Physics.OverlapBox(center, halfSize, transform.rotation);

        foreach ( Collider col in collidersInside)
        {
            if (col.CompareTag("Player")) return true;
            if (col.GetComponentInParent<ItemPickup>() != null) return true;
        }
        return false;
    }
    private IEnumerator RespawnAfterDelay(float delay)
    {   
        yield return new WaitForSeconds(delay);
        if (IsSpaceOccupied())
        {
            StartCoroutine(RespawnAfterDelay(2f));
            yield break;
        }
        else
        {
            _hitters.Clear();
            isBroken.Value = false;
        }
    }
    //runs on every client (and host), only the matching player raises the tutorial event
    [ClientRpc]
    private void NotifyBoxBrokenClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        //tutorial step to teach player to hit a loot box
        TutorialEvents.Raise(TutorialStep.HitBox);
    }
    public override void OnNetworkSpawn()
    {
        isBroken.OnValueChanged += OnBrokenChanged;
        ApplyBrokenState(isBroken.Value);
    }
    public override void OnNetworkDespawn()
    {
        isBroken.OnValueChanged -= OnBrokenChanged;
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }
}