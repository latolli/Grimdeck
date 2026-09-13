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
        //if (Mouse.current.leftButton.wasPressedThisFrame)
        //{
        //    Debug.Log("Combat mode: left click detected. Implement combat actions here.");
        //}
    }
}