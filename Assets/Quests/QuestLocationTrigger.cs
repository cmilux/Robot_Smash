using Unity.Netcode;
using UnityEngine;

public class QuestLocationTrigger : MonoBehaviour               //UPDATES PROGRESS OF A MISSION
{
    [SerializeField] private string locationId; // matches the targetId you set on the QuestObjective, e.g. "Watchtower"

    private bool alreadyReported = false; // prevents spamming ReportProgress every frame the player stays inside the trigger

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyReported) return; // only report once — this isn't a counter, it's a one-time "arrived" event
        if (!other.CompareTag("Player")) return;

        //only client who owns the car reports (otherwise every client watching it happen, will report too)
        //solo el cliente que es dueno del auto debe reportar (si no, cada auto que ve pasar el auto por el collider, tambien reporta el progreso)
        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if(netObj == null || !netObj.IsOwner) return;

        alreadyReported = true;
        QuestManager.Instance.ReportProgressServerRpc(ObjectiveType.ReachLocation, locationId, 1);
    }
}
