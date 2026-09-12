using UnityEngine;

[RequireComponent(typeof(QuestItemIdentity))]
public class QuestItem : MonoBehaviour, IClickable
{
    public Transform InteractionTarget => transform;
    public int InteractionRange => 1;

    private QuestItemIdentity questItemIdentity;

    private void Awake()
    {
        questItemIdentity = GetComponent<QuestItemIdentity>();
    }

    public void OnInteract()
    {
        QuestManager questManager = FindFirstObjectByType<QuestManager>();
        if (questManager != null)
        {
            Debug.Log("Collected quest item: " + gameObject.name);
            questManager.ReportItemCollected(questItemIdentity.Id);
        }
        else
        {
            Debug.LogError("QuestManager not found in the scene.");
        }
    }
}