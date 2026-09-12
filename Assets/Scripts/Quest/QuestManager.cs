using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestStatus
{
    public Quest quest;
    public int currentStepIndex = 0;
    public int currentStepProgress = 0;
}

public class QuestManager : MonoBehaviour
{
    public QuestCatalog questCatalog;
    public List<QuestStatus> activeQuests = new List<QuestStatus>();
    public List<Quest> completedQuests = new List<Quest>();

    public void StartQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= questCatalog.quests.Length)
        {
            Debug.LogError("Invalid quest index: " + questIndex);
            return;
        }

        Quest quest = questCatalog.quests[questIndex];

        if (activeQuests.Any(status => status.quest == quest) || completedQuests.Contains(quest))
        {
            Debug.Log("Quest already active or completed: " + quest.questName);
            return;
        }

        activeQuests.Add(new QuestStatus
        {
            quest = quest
        });

        Debug.Log("Added to active quests: " + quest.questName);
        Debug.Log("Current quest step: " + quest.steps[0].description);
    }

    private void CompleteCurrentStep(QuestStatus status)
    {
        status.currentStepIndex++;
        status.currentStepProgress = 0;

        if (status.currentStepIndex >= status.quest.steps.Length)
        {
            Debug.Log("Quest completed: " + status.quest.questName);
            completedQuests.Add(status.quest);
            activeQuests.Remove(status);
            return;
        }

        Debug.Log("New quest step: " +
                status.quest.steps[status.currentStepIndex].description);
    }

    public void ReportTalkToNPC(string npcId)
    {
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            QuestStatus status = activeQuests[i];
            QuestStep step = status.quest.steps[status.currentStepIndex];

            if (step.action == QuestAction.TalkToNPC &&
                step.targetId == npcId)
            {
                CompleteCurrentStep(status);
            }
        }
    }

    public void ReportEnemyKilled(string enemyId)
    {
        ReportProgress(QuestAction.KillEnemy, enemyId);
    }

    public void ReportItemCollected(string itemId)
    {
        ReportProgress(QuestAction.CollectItem, itemId);
    }

    private void ReportProgress(QuestAction action, string targetId)
    {
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            QuestStatus status = activeQuests[i];
            QuestStep step = status.quest.steps[status.currentStepIndex];

            if (step.action != action || step.targetId != targetId)
            {
                continue;
            }

            status.currentStepProgress++;

            if (status.currentStepProgress >= step.requiredAmount)
            {
                CompleteCurrentStep(status);
            }
        }
    }
}