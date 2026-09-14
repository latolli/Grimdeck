using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class CombatActions : MonoBehaviour
{
    public LayerMask clickableLayer;
    private CombatManager combatManager;

    void Start()
    {
        combatManager = CombatManager.Instance;
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

    public void CheckCardTarget(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
        {
            Debug.Log("Card target raycast missed.");
            return;
        }

        Debug.Log($"Card played to: {hit.point}, collider: {hit.collider.name}");

        IClickable clickable = hit.collider.GetComponentInParent<IClickable>();
        if (clickable == null)
        {
            Debug.LogWarning(
                $"Hit {hit.collider.name}, but it does not have an IClickable component.",
                hit.collider);
            return;
        }

        Debug.Log($"NPC / enemy hit: {hit.collider.name}");
    }
}