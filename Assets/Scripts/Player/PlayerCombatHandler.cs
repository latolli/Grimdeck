using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCombatHandler : MonoBehaviour
{
    public LayerMask clickableLayer;
    private CombatManager combatManager;
    private CardManager cardManager;

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

    public bool CheckCardTarget(Card card, Vector2 screenPosition)
    {
        bool cardPlayed = false;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
        {
            return false;
        }

        IEnemy enemy = hit.collider.GetComponentInParent<IEnemy>();
        if (enemy != null)
        {
            // TODO: Next, add real card effects:
            // Attacking enemy, gaining block to player
            // Draw, discard, destroy mechanisms
            int damage = Random.Range(3, 5);
            cardPlayed = enemy.OnCardTarget(CombatActionType.Attack, CombatEffectType.None, damage);
            // Callback that this card was played
            if (cardPlayed)
            {
                cardManager.PlayCard(card);
            }
        }

        return cardPlayed;
    }
}