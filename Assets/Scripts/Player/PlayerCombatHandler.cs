using UnityEngine;
using UnityEngine.InputSystem;

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
                combatManager.EndPlayerTurnCB();
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
                cardPlayed = enemy.OnCardTarget(
                    actionType,
                    action.CombatEffectTypes[i],
                    action.ActionValues[i]);
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

    public void TurnStartEffects()
    {
        playerCombatState.currentBlock = 0;
        combatManager.UpdateStatsPanelCB("0", playerCombatState, false);
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
}