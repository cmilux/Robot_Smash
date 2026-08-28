using UnityEngine;

public class QuestTrigger : MonoBehaviour               //STARTS THE MISSION
{
    [SerializeField] private int questIdToStart;

    private bool alreadyStarted = false; // stops re-triggering (and wiping progress) if the player walks through this zone again

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (alreadyStarted) return;

        alreadyStarted = true;
        QuestManager.Instance.StartQuestServerRpc(questIdToStart);
    }
}
