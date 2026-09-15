using UnityEngine;

// Change to json some day
public class EnemyCombatState
{
    public int maxHP;
    public int currentHP;
    public int currentBlock;
    public CombatAction[] actionPattern;     // Use this some beautiful day
    public bool isAlive;
}

[RequireComponent(typeof(NPCIdentity))]
public class RegularEnemy : MonoBehaviour, IEnemy
{
    public Transform InteractionTarget => transform;
    public int InteractionRange => 1;

    private NPCIdentity npcIdentity;
    public CombatEncounter combatEncounter;
    public EnemyCombatState enemyState;
    public int maxHP;

    private void Awake()
    {
        npcIdentity = GetComponent<NPCIdentity>();
    }

    public void OnInteract()
    {
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

    // Function to handle when card is played against this enemy
    public void OnCardTarget(CombatActionType action, CombatEffectType effect, int value)
    {
        if (enemyState == null)
        {
            Debug.LogError($"Enemy with ID '{npcIdentity}' doesn't have valid state");
            return;
        }

        // For now, just always do damage and have no other effects
        if (enemyState.isAlive)
        {
            enemyState.currentHP = Mathf.Min(
                enemyState.currentHP + enemyState.currentBlock - value,
                enemyState.currentHP
            );
            
            Debug.Log($"Enemy ID '{npcIdentity}' took {value} damage: HP = {enemyState.currentHP}");
            if (enemyState.currentHP <= 0)
            {
                enemyState.currentHP = 0;
                enemyState.isAlive = false;
                Debug.Log($"Enemy ID '{npcIdentity}' is dead");
            }
        }
        else
        {
            Debug.Log($"Enemy ID '{npcIdentity}' is already dead");
        }
    }

    public void ResetCombatState()
    {
        if (enemyState == null)
        {
            enemyState = new EnemyCombatState();
        }

        enemyState.maxHP = maxHP;
        enemyState.currentHP = maxHP;
        enemyState.currentBlock = 0;
        enemyState.isAlive = true;

        int patternLength = Random.Range(1, 4);
        enemyState.actionPattern = new CombatAction[patternLength];
        for (int i = 0; i < patternLength; i++)
        {
            enemyState.actionPattern[i] = new CombatAction();
        }

        Debug.Log($"Enemy ID '{npcIdentity}' ready for battle: HP = {enemyState.currentHP}");
        Debug.Log($"Attack pattern: {enemyState.actionPattern}");
    }

    public void NullifyCombatState()
    {
        enemyState = null;
    }
}
