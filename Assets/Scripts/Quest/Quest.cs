using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    public string questGiverId;
    [TextArea] public string questDescription;
    public QuestStep[] steps;
}

[Serializable]
public class QuestStep
{
    [TextArea] public string description;
    public QuestAction action;
    public string targetId;
    public int requiredAmount = 1;
}

public enum QuestAction
{
    TalkToNPC,
    KillEnemy,
    CollectItem
}