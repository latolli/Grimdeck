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
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            combatManager.EndCombat();
        }
    }

    public void CheckCardTarget(Card card, Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
        {
            Debug.Log("Card target raycast missed.");
            return;
        }

        IEnemy enemy = hit.collider.GetComponentInParent<IEnemy>();
        if (enemy != null)
        {
            // For now, always just attack
            int damage = Random.Range(3, 5);
            bool cardPlayed = enemy.OnCardTarget(CombatActionType.Attack, CombatEffectType.None, damage);
            // Callback that this card was played
            if (cardPlayed)
            {
                cardManager.PlayCard(card);
            }
        }

    }
}