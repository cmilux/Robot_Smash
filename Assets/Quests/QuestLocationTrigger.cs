using UnityEngine;

public class QuestLocationTrigger : MonoBehaviour               //UPDATES PROGRESS OF A MISSION
{
    [SerializeField] private string locationId; // matches the targetId you set on the QuestObjective, e.g. "Watchtower"

    private bool alreadyReported = false; // prevents spamming ReportProgress every frame the player stays inside the trigger

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (alreadyReported) return; // only report once — this isn't a counter, it's a one-time "arrived" event

        alreadyReported = true;
        QuestManager.Instance.ReportProgressServerRpc(ObjectiveType.ReachLocation, locationId, 1);
    }
}
