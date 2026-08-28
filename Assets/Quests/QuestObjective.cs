using UnityEngine;

public enum ObjectiveType
{
    //Types of objectives the quests can have
    KillEnemy,      //kill x amount of enemies
    CollectItem,    //pick up x amount of an item
    DestroyObject,  //break x amount of loot boxes
    ReachLocation   //enter or reach a zone
}

//One objective inside a quest. This is data only — no runtime state lives here,
// because ScriptableObjects are SHARED ASSETS: if you stored "currentAmount" on this
// object itself, both players (and every quest instance using it) would read/write
// the same value. Runtime progress goes in QuestManager instead (see below).
[CreateAssetMenu(fileName = "New Objective", menuName = "Quests/Objective")]
public class QuestObjective : ScriptableObject
{
    public ObjectiveType type;                  //type of obj
    public string targetId;                     //enemy tag, itemdata id or location id
    public int requiredAmount = 1;             //how many kills/items/etc are needed
    [TextArea] public string description;       //shown in the quest ui

    public bool isOptional = false;             //if true, this obj doesnt block quest completion (bonus)

    public int bonusExp = 0;                    //extra reward granted only if the optional obj is completed
}
