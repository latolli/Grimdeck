using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerCombatHandler : MonoBehaviour
{
    public LayerMask clickableLayer;
    private CombatManager combatManager;
    private CardManager cardManager;

    private CombatStats playerCombatState;

    void Start()
    {
        combatManager = CombatManager.Instance;
        cardManager = FindFirstObjectByType<CardManager>();;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Esc ends combat in any state
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            combatManager.EndCombatCB(CombatResult.Escape);
        }
        // Check other actions allowed only in player's turn
        else if (combatManager.combatState == CombatState.PlayerTurn)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                // End turn
                TurnEndEffects();
            }    
        }
    }

    public void CheckCardTarget(Card card, Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
        {
            return;
        }

        IEnemy enemy = hit.collider.GetComponentInParent<IEnemy>();
        CombatAction action = card.Action;
        bool playerHit = hit.collider.GetComponentInParent<PlayerCombatHandler>() == this;
        bool hasOffensiveEffect = false;

        for (int i = 0; i < action.ActionTypes.Length; i++)
        {
            CombatActionType actionType = action.ActionTypes[i];
            hasOffensiveEffect |= actionType == CombatActionType.Attack ||
                                  actionType == CombatActionType.ApplyDebuff;
        }

        if (enemy == null && (!playerHit || hasOffensiveEffect))
        {
            return;
        }

        bool cardPlayed = playerHit && !hasOffensiveEffect;
        // Play all combat effects from card
        for (int i = 0; i < action.ActionTypes.Length; i++)
        {
            CombatActionType actionType = action.ActionTypes[i];
            // Attack or apply debuff to enemy
            if (actionType == CombatActionType.Attack ||
              actionType == CombatActionType.ApplyDebuff)
            {
                // Apply potential adjustments (mainly weaken)
                int adjustedValue = action.ActionValues[i];
                if (actionType == CombatActionType.Attack &&
                    playerCombatState.statusEffects.Weakened > 0)
                {
                    adjustedValue = (int)Math.Ceiling(adjustedValue * 0.75);
                }
                cardPlayed = enemy.OnCardTarget(
                    actionType,
                    action.CombatEffectTypes[i],
                    adjustedValue);
            }
            // Heal or block player if card was played
            // Offensive actions always need to happen first in order for this to work
            else if (actionType == CombatActionType.Heal && cardPlayed)
            {
                playerCombatState.currentHP = Mathf.Min(playerCombatState.maxHP,
                    playerCombatState.currentHP + action.ActionValues[i]);
                Debug.Log($"Healed player for {action.ActionValues[i]}");
            }
            else if (actionType == CombatActionType.Block && cardPlayed)
            {
                playerCombatState.currentBlock += action.ActionValues[i];
                Debug.Log($"Blocked player for {action.ActionValues[i]}");
            }
        }
        if (cardPlayed)
        {
            cardManager.PlayCard(card);
            combatManager.UpdateStatsPanelCB("0", playerCombatState, false);
        }
    }

    // TODO: This is basically identical to the enemy side function
    public bool TurnStartEffects()
    {
        bool playerAlive = true;
        // Reset block and loop active effects
        playerCombatState.currentBlock = 0;
        if (playerCombatState.statusEffects.Poisoned > 0)
        {
            playerCombatState.currentHP = Mathf.Max(0, playerCombatState.currentHP - playerCombatState.statusEffects.Poisoned);
            playerCombatState.statusEffects.Poisoned--;
        }

        if (playerCombatState.statusEffects.OnFire > 0)
        {
            playerCombatState.currentHP = Mathf.Max(0, playerCombatState.currentHP - 2);
            playerCombatState.statusEffects.OnFire--;
        }

        // Check if player still alive
        if (playerCombatState.currentHP <= 0)
        {
            playerCombatState.isAlive = false;
            playerAlive = false;
        }

        combatManager.UpdateStatsPanelCB("0", playerCombatState, false);
        return playerAlive;
    }

    public void ResetPlayerCombatState()
    {
        if (playerCombatState == null)
        {
            playerCombatState = new CombatStats();
        }

        playerCombatState.maxHP = 20;
        playerCombatState.currentHP = 20;
        playerCombatState.currentBlock = 0;
        playerCombatState.statusEffects = new EffectStatus();
        playerCombatState.isAlive = true;
        combatManager.UpdateStatsPanelCB("0", playerCombatState, false);
    }

    // TODO: This is basically identical to the one used in enemy side -> make one common function
    public void ApplyActionToPlayer(CombatActionType action, CombatEffectType effect, int value)
    {
        // Check all invalid actions
        if (playerCombatState == null || !playerCombatState.isAlive || value < 0)
        {
            return;
        }

        switch (action)
        {
            case CombatActionType.Attack:
                playerCombatState.currentHP = Mathf.Max(0,
                    playerCombatState.currentHP - Mathf.Max(0, value - playerCombatState.currentBlock)
                );
                playerCombatState.currentBlock = Mathf.Max(0, playerCombatState.currentBlock - value);
                Debug.Log($"Player took {value} damage: HP = {playerCombatState.currentHP}");
                break;

            case CombatActionType.ApplyDebuff:
                switch (effect)
                {
                    case CombatEffectType.Poison:
                        playerCombatState.statusEffects.Poisoned += value;
                        break;
                    case CombatEffectType.Fire:
                        playerCombatState.statusEffects.OnFire += value;
                        break;
                    case CombatEffectType.Weaken:
                        playerCombatState.statusEffects.Weakened += value;
                        break;
                    default:
                        Debug.LogError($"Unsupported debuff effect '{effect}' for player.");
                        return;
                }
                break;

            default:
                Debug.LogError($"Unsupported card action '{action}' for player.");
                return;
        }

        // Player lost
        if (playerCombatState.currentHP <= 0)
        {
            playerCombatState.currentHP = 0;
            playerCombatState.isAlive = false;
            combatManager.EndCombatCB(CombatResult.Defeat);
            return;
        }

        // Update stats panel
        combatManager.UpdateStatsPanelCB("0", playerCombatState, false);
    }

    private void TurnEndEffects()
    {
        // Decrease weak status
        if (playerCombatState.statusEffects.Weakened > 0)
        {
            playerCombatState.statusEffects.Weakened--;
        }
        // Update stats panel
        combatManager.UpdateStatsPanelCB("0", playerCombatState, false);
        combatManager.EndPlayerTurnCB();
    }
}