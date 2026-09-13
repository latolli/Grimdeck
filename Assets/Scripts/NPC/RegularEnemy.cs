using UnityEngine;

[RequireComponent(typeof(NPCIdentity))]
public class RegularEnemy : MonoBehaviour, IClickable
{
    public Transform InteractionTarget => transform;
    public int InteractionRange => 1;

    private NPCIdentity npcIdentity;
    public CombatEncounter combatEncounter;

    private void Awake()
    {
        npcIdentity = GetComponent<NPCIdentity>();
    }

    public void OnInteract()
    {
        Debug.Log("Argh! I am " + gameObject.name + ". Prepare to die!");
        QuestManager questManager = FindFirstObjectByType<QuestManager>();
        if (questManager != null)
        {
            questManager.ReportEnemyKilled(npcIdentity.Id);
        }
        else
        {
            Debug.LogError("QuestManager not found in the scene.");
        }

        // Start combat when the player interacts with the enemy
        CombatManager combatManager = FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            if (combatEncounter != null)
            {
                // Here you can set up the combat encounter using the combatEncounter data
                // For example, you might want to spawn enemies based on the encounter data
                combatManager.StartCombat(combatEncounter);
            }
            else
            {
                Debug.Log("CombatEncounter not assigned for " + gameObject.name);
            }
        }
        else
        {
            Debug.LogError("CombatManager not found in the scene.");
        }
    }
}
