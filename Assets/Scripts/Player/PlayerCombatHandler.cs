using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCombatState
{
    public int maxHP;
    public int currentHP;
    public int currentBlock;
    public EffectStatus statusEffects;
    public bool isAlive;
}

public class PlayerCombatHandler : MonoBehaviour
{
    public LayerMask clickableLayer;
    private CombatManager combatManager;
    private CardManager cardManager;

    private PlayerCombatState playerCombatState;

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

        // Play all combat effects from card
        IEnemy enemy = hit.collider.GetComponentInParent<IEnemy>();
        CombatAction action = card.Action;
        for (int i = 0; i < action.ActionTypes.Length; i++)
        {
            CombatActionType actionType = action.ActionTypes[i];
            // Attack or apply debuff to enemy
            if (actionType == CombatActionType.Attack ||
              actionType == CombatActionType.ApplyDebuff)
            {
                enemy.OnCardTarget(
                    actionType,
                    action.CombatEffectTypes[i],
                    action.ActionValues[i]);
            }
            // Heal or block player
            else if (actionType == CombatActionType.Heal)
            {
                playerCombatState.currentHP = Mathf.Min(playerCombatState.maxHP,
                    playerCombatState.currentHP + action.ActionValues[i]);
                Debug.Log($"Healed player for {action.ActionValues[i]}");
            }
            else if (actionType == CombatActionType.Block)
            {
                playerCombatState.currentBlock += action.ActionValues[i];
                Debug.Log($"Blocked player for {action.ActionValues[i]}");
            }
        }
        cardManager.PlayCard(card);
    }

    public void ResetPlayerCombatState()
    {
        if (playerCombatState == null)
        {
            playerCombatState = new PlayerCombatState();
        }

        playerCombatState.maxHP = 20;
        playerCombatState.currentHP = 20;
        playerCombatState.currentBlock = 0;
        playerCombatState.statusEffects = new EffectStatus();
        playerCombatState.isAlive = true;
    }
}