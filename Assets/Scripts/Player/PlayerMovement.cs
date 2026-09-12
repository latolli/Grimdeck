using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public LayerMask clickableLayer;

    private NavMeshAgent agent;
    private GridSystem grid;
    private CombatManager combatManager;
    private IClickable pendingInteraction;
    private Vector2Int targetTile;
    private Vector3 pendingCombatCenter;
    private bool hasPendingCombatPreparation;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        grid = GridSystem.Instance;
        combatManager = CombatManager.Instance;
    }

    void Update()
    {
        HandleInput();
        CheckArrival();
    }

    void HandleInput()
    {
        if (combatManager.gameState == GameState.InCombat)
        {
            HandleCombatInput();
        }
        else if (combatManager.gameState == GameState.Free)
        {
            HandleFreeWroldInput();
        }
    }

    // TODO: This is a bit of a hack, but it works for now. We should eventually implement a proper pathfinding solution that respects the grid and obstacles.
    Vector3 FindClosestReachableTile(Vector3 startPos, Vector3 targetPos, int range)
    {
        List<Vector3> candidates = grid.GetTilesInRange(targetPos, range);

        Vector3 closestTile = Vector3.zero;
        float distance = float.MaxValue;
        bool found = false;

        for (int i = 0; i < candidates.Count; i++)
        {
            if (!NavMesh.SamplePosition(candidates[i], out NavMeshHit navHit, grid.tileSize * 1f, NavMesh.AllAreas))
                continue; // not walkable, skip


            float candidateDistance = Vector3.Distance(startPos, navHit.position);
            if (candidateDistance < distance)
            {
                closestTile = navHit.position;
                distance = candidateDistance;
                found = true;
            }
        }

        return found ? closestTile : targetPos; // fallback: don't move if nothing reachable
    }

    void CheckArrival()
    {
        // Regular movement click
        if (combatManager.gameState == GameState.Free)
        {
            agent.updateRotation = true;
            if (pendingInteraction == null) return;
            if (agent.pathPending) return; // path still calculating

            // Check if we arrived at the target tile
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                pendingInteraction.OnInteract(); 
                pendingInteraction = null;
            }
        }
        // Combat preparation ongoing
        else if (combatManager.gameState == GameState.PreparingCombat)
        {
            if (agent.pathPending) return; // path still calculating

            // Check if we arrived at the target tile
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.updateRotation = false;
                // Rotate player to face towards combat center
                if (hasPendingCombatPreparation)
                {
                    Vector3 direction = (pendingCombatCenter - transform.position).normalized;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.RotateTowards(
                            transform.rotation,
                            targetRotation,
                            agent.angularSpeed * Time.deltaTime);

                        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
                            return;
                    }
                }
                pendingCombatCenter = Vector3.zero;
                hasPendingCombatPreparation = false;
                combatManager.CombatPreparingReady();
            }
        }
        else
        {
            // In combat, we don't handle arrival for now
            return;
        }
    }

    void HandleCombatInput()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            combatManager.EndCombat();
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Combat mode: left click detected. Implement combat actions here.");
        }
    }

    void HandleFreeWroldInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
            {
                // Any new click cancels whatever was pending before
                pendingInteraction = null;

                if (hit.collider.TryGetComponent<IClickable>(out var clickable))
                {
                    // Move toward the target, remember what to do on arrival
                    Vector3 closestTile = 
                        FindClosestReachableTile(
                            transform.position,
                            clickable.InteractionTarget.position,
                            clickable.InteractionRange);
                    agent.SetDestination(closestTile);
                    targetTile = grid.WorldToGrid(closestTile);
                    pendingInteraction = clickable;
                }
                else
                {
                    // Plain floor click — just move
                    Vector3 snapped = grid.SnapToTileCenter(hit.point);
                    agent.SetDestination(snapped);
                }
            }
        }
    }

    public void MovePlayerToTile(Vector3 gridPos, Vector3 combatCenter)
    {
        pendingInteraction = null;
        pendingCombatCenter = combatCenter;
        hasPendingCombatPreparation = true;
        Vector3 worldPos = grid.SnapToTileCenter(gridPos);
        agent.SetDestination(worldPos);
    }
}