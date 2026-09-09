using Unity.Netcode;
using UnityEngine;

public class QuestLocationTrigger : NetworkBehaviour               //UPDATES PROGRESS OF A MISSION
{
    [SerializeField] ObjectiveType objectiveType = ObjectiveType.ReachLocation;     //ReachLocation para atalaya/bunker, CollectItem para la carta
    [SerializeField] string targetId; // matches the targetId you set on the QuestObjective, e.g. "Watchtower"

    [SerializeField] bool destroySelfOnTrigger = false;

    private bool alreadyReported = false; // prevents spamming ReportProgress every frame the player stays inside the trigger

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyReported) return; // only report once — this isn't a counter, it's a one-time "arrived" event
        if (!other.CompareTag("Player")) return;

        //only client who owns the car reports (otherwise every client watching it happen, will report too)
        //solo el cliente que es dueno del auto debe reportar (si no, cada auto que ve pasar el auto por el collider, tambien reporta el progreso)
        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if(netObj == null || !netObj.IsOwner) return;

        // check before consuming — lets the player retry later if the order isn't right yet
        // chequea antes de consumir — le permite al jugador reintentar despues si el orden no esta listo todavia
        if (!QuestManager.Instance.CanReportProgress(objectiveType, targetId)) return;

        alreadyReported = true;
        QuestManager.Instance.ReportProgressServerRpc(objectiveType, targetId, 1);

        // for narrative pickups like the letter — remove the object once it's been "collected"
        // para pickups narrativos como la carta — elimina el objeto una vez "recolectado"
        if (destroySelfOnTrigger && IsServer && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(false);
        }
    }
}
