using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class QuestManager : NetworkBehaviour
{
    //static singleton, any script can call QuestManager.Instance
    public static QuestManager Instance { get; private set; }

    //every quest in the game, dragged in via inspector
    [SerializeField] private List<QuestData> allQuest;

    //which quest is currently active (server writes, everyone reads)
    private NetworkVariable<int> activeQuestId = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    //progress per objective, index-matched to activeQuest.objectives
    //a NetworkList syncs each element change to clients automatically
    //progreso por objetivo, el indice coincide con activeQuest.objectives
    //un NetworkList sincroniza cada cambio de elemento a los clientes automaticamente
    private NetworkList<int> objectiveProgress = new NetworkList<int>();

    //convenience lookup for the currently active quest's data
    //busqueda rapida de los datos de la mision activa actual
    private QuestData ActiveQuest => allQuest.Find(q => q.questId == activeQuestId.Value);


    private void Awake()
    {
        Instance = this;    //register singleton
    }

    public override void OnNetworkSpawn()
    {
        //whener the active quest changes, tell UI to refresh
        //activeQuestId.OnValueChanged += (oldId, newId) => QuestUI.Instance?.RefreshQuest(ActiveQuest, objectiveProgress);

        //whenever any objective's progress count changes, tell UI to refresh too
        //objectiveProgress.OnListChanged += (change) => QuestUI.Instance?.RefreshQuest(ActiveQuest, objectiveProgress);
    }

    //server start quest (this needs to be called from the mission giver(npc), a trigger zone, etc)
    //el server inicia la mision(esto se llama desde el npc que da la mision, una zona de trigger, etc)
    [Rpc(SendTo.Server)]
    public void StartQuestServerRpc(int questId)
    {
        activeQuestId.Value = questId;      //set the new active quest

        objectiveProgress.Clear();          //wipe old progress
        QuestData quest = allQuest.Find(q => q.questId ==  questId);
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            objectiveProgress.Add(0);   //one progress counter per obj (starts at 0)
        }
    }

    //server side game events call this directly (runs only in server)
    //los eventos del lado del server llaman esto directamente
    public void ReportProgress(ObjectiveType type, string targetId, int amount = 1)
    {
        if (!IsServer) return;                  //safety net, only server report progress
        ApplyProgress(type, targetId, amount);
    }

    //client side callers use this instead (goes through the server via RPC)
    //los que llaman desde el cliente usan esto en vez de lo anterior (pasa por el server via RPC)
    [Rpc(SendTo.Server)]
    public void ReportProgressServerRpc(ObjectiveType type, string targetId, int amount = 1)
    {
        //this RPC body always executes on the server. no IsServer check needed here
        //el cuerpo de este RPC siempre corre en el server, no hace falta chequear IsServer aca
        ApplyProgress(type, targetId, amount);
    }

    //shared logic both entry points (ReportProgress and ReportProgressServerRpc) funnel into
    //logica compartida a la que llegan las dos entradas de arriba (ReportProgress y ReportProgressServerRpc)
    private void ApplyProgress(ObjectiveType type, string targetId, int amount)
    {
        if (ActiveQuest == null) return;       //no active quest, nothing to update

        if (ActiveQuest != null)
        {
            Debug.Log("Quest is active");
        }


        for (int i = 0; i < ActiveQuest.objectives.Count; i++)
        {
            QuestObjective obj = ActiveQuest.objectives[i];

            //only bump progress if this event matches the obj's type and target
            //solo suma progreso si este evento coincide con el tipo y el target del objetivo
            if (obj.type == type && obj.targetId == targetId)
            {
                //Mathf.Min caps progress at the required amount so it can't overshoot
                //Mathf.Min limita el progreso a la cantidad requerida para que no se pase
                int newProgress = Mathf.Min(objectiveProgress[i] + amount, obj.requiredAmount);
                objectiveProgress[i] = newProgress; //setting NetList[i] auto sync to client | asignar NetList[i] sincroniza automaticamente a los clientes
            }

        }

        CheckQuestComplete();
    }

    private void CheckQuestComplete()
    {
        //only checks main objectives, decides if the quest can close
        for (int i = 0; i < ActiveQuest.objectives.Count; ++i)
        {
            QuestObjective obj = ActiveQuest.objectives[i];

            if (obj.isOptional) continue; //if the objective is optional, wont stop the progress of the main objective

            if (objectiveProgress[i] < obj.requiredAmount)
            {
                return;     //at least one obj isnt done yet, bail out | al menos un objetivo no esta completo, se corta aca
            }
        }

        int totalExp = ActiveQuest.rewardExp;   //if main obj is completed, the quest is done

        //only runs if the main objective is done, now checks if the optional objective was completed to sum extra bonus
        for (int i = 0; i < ActiveQuest.objectives.Count; ++i)
        {
            QuestObjective obj = ActiveQuest.objectives[i];

            //only add bonus exp if the main and optional objective were completed
            if (obj.isOptional && objectiveProgress[i] >= obj.requiredAmount)
            {
                totalExp += obj.bonusExp;   //sum the bonus exp only if completed
            }
        }

        GrantRewardsClientRpc(totalExp);   //all obj done (tell clients to grant rewards)

        //turn the quest off so no future progress report can trigger this
        //apaga la mision para que ningun futuro reporte dispare esto (evita duplicados)
        activeQuestId.Value = -1;

        Debug.Log("Quest is done");
    }

    //runs on every client — each client grants rewards to their own local player only
    //corre en cada cliente — cada cliente le otorga las recompensas solo a su propio jugador local
    [ClientRpc]
    private void GrantRewardsClientRpc(int expAmoun)
    {
        PlayerLevelUI.Instance.AddExp(expAmoun);    //sum exp

        //find this client's own player network object | busca el network object del jugador de este cliente
        NetworkObject localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayerObj == null) return;

        //find this client's own inventory | busca el inventario este cliente
        Inventory localInventory = localPlayerObj.GetComponent<Inventory>();
        if (localInventory == null) return;

        // add each reward item to the local player's inventory | agrega cada item de recompensa al inventario del jugador local
        foreach (ItemData reward in ActiveQuest.rewardItems)
        {
            localInventory.AddItem(reward, 1);
        }
    }
}
