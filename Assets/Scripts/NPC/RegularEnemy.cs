using UnityEngine;
using System;

[RequireComponent(typeof(NPCIdentity))]
public class RegularEnemy : MonoBehaviour, IEnemy
{
    public Transform InteractionTarget => transform;
    public int InteractionRange => 1;

    private NPCIdentity npcIdentity;
    public CombatEncounter combatEncounter;
    public CombatStats enemyState;
    private CombatAction[] actionPattern;     // Use this some beautiful day
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
    public bool OnCardTarget(CombatActionType action, CombatEffectType effect, int value)
    {
        // Check all invalid actions
        if (enemyState == null || !enemyState.isAlive || value < 0)
        {
            return false;
        }

        switch (action)
        {
            case CombatActionType.Attack:
                enemyState.currentHP = Mathf.Max(0,
                    enemyState.currentHP - Mathf.Max(0, value - enemyState.currentBlock)
                );
                enemyState.currentBlock = Mathf.Max(0, enemyState.currentBlock - value);
                Debug.Log($"Enemy ID '{npcIdentity}' took {value} damage: HP = {enemyState.currentHP}");
                break;

            case CombatActionType.ApplyDebuff:
                switch (effect)
                {
                    case CombatEffectType.Poison:
                        enemyState.statusEffects.Poisoned += value;
                        break;
                    case CombatEffectType.Fire:
                        enemyState.statusEffects.OnFire += value;
                        break;
                    case CombatEffectType.Weaken:
                        enemyState.statusEffects.Weakened += value;
                        break;
                    default:
                        Debug.LogError($"Unsupported debuff effect '{effect}' for enemy ID '{npcIdentity}'.");
                        return false;
                }
                break;

            default:
                Debug.LogError($"Unsupported card action '{action}' for enemy ID '{npcIdentity}'.");
                return false;
        }

        CombatManager combatManager = FindFirstObjectByType<CombatManager>();
        if (enemyState.currentHP <= 0)
        {
            enemyState.currentHP = 0;
            enemyState.isAlive = false;
            if (combatManager != null)
            {
                combatManager.EnemyKilledCB(npcIdentity.Id);
            }
            else
            {
                Debug.LogError("CombatManager not found while resolving enemy damage.");
            }
        }

        // Update stats panel
        combatManager.UpdateStatsPanelCB(npcIdentity.Id, enemyState, true);

        return true;
    }

    public void PlayEnemyTurn(int turnCounter)
    {
        if (enemyState == null || !enemyState.isAlive)
        {
            return;
        }

        CombatManager combatManager = FindFirstObjectByType<CombatManager>();

        // Block goes to 0
        enemyState.currentBlock = 0;

        // Apply possibly lethal effects
        if (enemyState.statusEffects.Poisoned > 0)
        {
            enemyState.currentHP = Mathf.Max(0, enemyState.currentHP - enemyState.statusEffects.Poisoned);
            enemyState.statusEffects.Poisoned--;
        }
        if (enemyState.statusEffects.OnFire > 0)
        {
            enemyState.currentHP = Mathf.Max(0, enemyState.currentHP - 2);
            enemyState.statusEffects.OnFire--;
        }

        // Check if enemy still alive
        if (enemyState.currentHP <= 0)
        {
            enemyState.isAlive = false;
            if (combatManager != null)
            {
                combatManager.EnemyKilledCB(npcIdentity.Id);
            }
            else
            {
                Debug.LogError("CombatManager not found while resolving enemy status effects.");
            }
        }
        else
        {
            // Play enemy's turn
            int patternIdx = turnCounter % actionPattern.Length;
            CombatAction action = actionPattern[patternIdx];
            for (int i = 0; i < action.ActionTypes.Length; i++)
            {
                CombatActionType actionType = action.ActionTypes[i];
                // Attack or apply debuff
                if (actionType == CombatActionType.Attack ||
                actionType == CombatActionType.ApplyDebuff)
                {
                    combatManager.EnemyActionCB(
                        actionType,
                        action.CombatEffectTypes[i],
                        action.ActionValues[i]);
                }
                // Heal enemy
                else if (actionType == CombatActionType.Heal)
                {
                    enemyState.currentHP = Mathf.Min(enemyState.maxHP,
                        enemyState.currentHP + action.ActionValues[i]);
                    Debug.Log($"Healed enemy {npcIdentity.Id} for {action.ActionValues[i]}");
                }
                // Block enemy
                else if (actionType == CombatActionType.Block)
                {
                    enemyState.currentBlock += action.ActionValues[i];
                    Debug.Log($"Blocked enemy {npcIdentity.Id} for {action.ActionValues[i]}");
                }
            }
            
            // Decrease weak status
            if (enemyState.statusEffects.Weakened > 0)
            {
                enemyState.statusEffects.Weakened--;
            }
        }
        // Update stats panel
        combatManager.UpdateStatsPanelCB(npcIdentity.Id, enemyState, true);
    }

    public void ResetEnemyCombatState()
    {
        if (enemyState == null)
        {
            enemyState = new CombatStats();
        }

        enemyState.maxHP = maxHP;
        enemyState.currentHP = maxHP;
        enemyState.currentBlock = 0;
        enemyState.statusEffects = new EffectStatus();
        enemyState.isAlive = true;

        int patternLength = (int)Math.Ceiling(maxHP / 3.0f);
        actionPattern = new CombatAction[patternLength];
        for (int i = 0; i < patternLength; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, 5);
            actionPattern[i] = DummyActionPattern(randomNum);
        }
        // Update stats panel
        CombatManager combatManager = FindFirstObjectByType<CombatManager>();
        combatManager.UpdateStatsPanelCB(npcIdentity.Id, enemyState, true);
    }

    public void NullifyCombatState()
    {
        enemyState = null;
    }

    // Function to generate actions for enemies
    // Temp solution, each enemy will have unique patterns later
    private static CombatAction DummyActionPattern(int index)
    {
        CombatAction action = new CombatAction();
        if (index == 0)
        {
            action.ActionTypes = new[] { CombatActionType.Block };
            action.CombatEffectTypes = new[] { CombatEffectType.None };
            action.ActionValues = new[] { 3 };
        }
        else if (index == 1)
        {
            action.ActionTypes = new[] { CombatActionType.Attack };
            action.CombatEffectTypes = new[] { CombatEffectType.None };
            action.ActionValues = new[] { 2 };
        }
        else if (index == 2)
        {
            action.ActionTypes = new[] { CombatActionType.Attack, CombatActionType.ApplyDebuff };
            action.CombatEffectTypes = new[] { CombatEffectType.None, CombatEffectType.Poison };
            action.ActionValues = new[] { 2, 3 };
        }
        else if (index == 3)
        {
            action.ActionTypes = new[] { CombatActionType.Block, CombatActionType.ApplyDebuff };
            action.CombatEffectTypes = new[] { CombatEffectType.None, CombatEffectType.Fire };
            action.ActionValues = new[] { 1, 3 };
        }
        else
        {
            action.ActionTypes = new[] { CombatActionType.Heal, CombatActionType.ApplyDebuff };
            action.CombatEffectTypes = new[] { CombatEffectType.None, CombatEffectType.Weaken };
            action.ActionValues = new[] { 2, 3 };
        }
        return action;
    }
}
