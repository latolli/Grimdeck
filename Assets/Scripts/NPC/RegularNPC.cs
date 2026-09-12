using UnityEngine;

[RequireComponent(typeof(NPCIdentity))]
public class RegularNPC : MonoBehaviour, IClickable
{
    public Transform InteractionTarget => transform;
    public int InteractionRange => 1;
    public Quest questToOffer;

    private NPCIdentity npcIdentity;

    private void Awake()
    {
        npcIdentity = GetComponent<NPCIdentity>();
    }

    public void OnInteract()
    {
        QuestManager questManager = FindFirstObjectByType<QuestManager>();
        if (questManager != null)
        {
            questManager.ReportTalkToNPC(npcIdentity.Id);
            if (questToOffer != null)
            {
                // TODO: Implement a proper quest offering system (e.g., UI prompt) instead of automatically starting the quest.
                int questIndex = System.Array.IndexOf(questManager.questCatalog.quests, questToOffer);
                if (questIndex >= 0)
                {
                    questManager.StartQuest(questIndex);
                }
                else
                {
                    Debug.LogError("Quest not found in catalog: " + questToOffer.questName);
                }
            }
        }
        else
        {
            Debug.LogError("QuestManager not found in the scene.");
        }
    }
}
