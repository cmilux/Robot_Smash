using UnityEngine;

public class QuestTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        QuestManager.Instance.StartQuestServerRpc(0);

        Debug.Log("Car trigger w mission");
    }
}
