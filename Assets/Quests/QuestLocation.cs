using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class QuestLocation : NetworkBehaviour               //UPDATES PROGRESS OF A MISSION
{
    [SerializeField] ObjectiveType objectiveType = ObjectiveType.ReachLocation;     //ReachLocation para atalaya/bunker, CollectItem para la carta
    [SerializeField] string targetId; // matches the targetId you set on the QuestObjective, e.g. "Watchtower"
    [SerializeField] GameObject[] turnGameObject;
    [SerializeField] bool destroySelfOnTrigger = false;

    private bool alreadyReported = false; // prevents spamming ReportProgress every frame the player stays inside the trigger

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TryReport(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (!collision.gameObject.CompareTag("MeleeWeapon")) return;
        if (!collision.collider.CompareTag("MeleeWeapon")) return;
        Debug.Log($"[QuestLocation] {collision.gameObject.name} collided with {gameObject.name}");
        TryReport(collision.collider.gameObject);    
    }

    private void TryReport(GameObject source)
    {
        if (alreadyReported) return; // only report once — this isn't a counter, it's a one-time "arrived" event

        //only client who owns the car reports (otherwise every client watching it happen, will report too)
        //solo el cliente que es dueno del auto debe reportar (si no, cada auto que ve pasar el auto por el collider/trigger, tambien reporta el progreso)
        NetworkObject netObj = source.gameObject.GetComponentInParent<NetworkObject>();
        if (netObj == null || !netObj.IsOwner) return;

        // check before consuming — lets the player retry later if the order isn't right yet
        // chequea antes de consumir — le permite al jugador reintentar despues si el orden no esta listo todavia
        if (!QuestManager.Instance.CanReportProgress(objectiveType, targetId)) return;

        alreadyReported = true;
        CompletedObjectivesServerRpc();
    }

    // the server does the report, the toggle and the despawn, so every client sees the result
    // el servidor realiza el reporte, apaga y prende, asi cada cliente ve el resultado
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CompletedObjectivesServerRpc()
    {
        QuestManager.Instance.ReportProgressServerRpc(objectiveType, targetId, 1);

        ToggleObjects();

        // for narrative pickups like the letter — remove the object once it's been "collected"
        // para pickups narrativos como la carta — elimina el objeto una vez "recolectado"
        if (destroySelfOnTrigger && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(false);
        }
    }

    private void ToggleObjects()
    {
        //turn on/off an object after completing an objective
        if (turnGameObject == null) return;

        foreach (GameObject obj in turnGameObject)
        {
            if (obj == null) continue;

            var componentController = obj.GetComponent<ComponentController>();
            if (componentController == null) continue;
            componentController.SetEnabled(!componentController.EnabledState);  //EnabledState is part of the ComponentController library and controls on/off game logic. enabled is from MonoBehaviour and controls if the scripts run
        }
    }
}
