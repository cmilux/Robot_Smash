using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestData : ScriptableObject
{
    public int questId;                         //unique ID (like ItemData)
    public string questTitle;                   //quest title
    [TextArea] public string description;       //quest description
    public List<QuestObjective> objectives;     //one quest can need multiple obj done

    public int requiredQuestId = -1;            //prerequisite quest id, -1 = no prerequisite

    public List<ItemData> rewardItems;          //items granted on completion
    public int rewardExp;                       //exp granted on completion (connected w PlayerLevelUI)
}
