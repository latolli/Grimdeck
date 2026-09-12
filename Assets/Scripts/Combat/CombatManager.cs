using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Free,
    InCombat,
    PreparingCombat
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public GameState gameState = GameState.Free;
    UIManager uiManager;
    public CombatCatalog combatCatalog;
    private CombatEncounter currentEncounter;
    private PlayerMovement playerMovement;
    private OrbitCamera orbitCamera;
    public int numPlayers = 1;  // Hardcoded for now, can be set dynamically later

    // TODO: Fix default positions, these assume that combat is oriented same way every time
    public Vector3[] defaultEnemyPositions = new Vector3[]
    {
        new Vector3(2, 0, 0),
        new Vector3(3, 0, -2),
        new Vector3(3, 0, 2)
    };

    public Vector3[] defaultPlayerPositions = new Vector3[]
    {
        new Vector3(-2, 0, 0),
        new Vector3(-3, 0, -2),
        new Vector3(-3, 0, 2)
    };

    void Awake() => Instance = this;

    void Start()
    {
        uiManager = UIManager.Instance;
        orbitCamera = FindFirstObjectByType<OrbitCamera>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    // Change game and camera state to combat mode
    public void StartCombat(CombatEncounter encounter)
    {
        if (encounter == null)
        {
            Debug.LogError("Cannot start combat without an encounter.");
            return;
        }

        if (playerMovement == null || orbitCamera == null || uiManager == null)
        {
            Debug.LogError("CombatManager doesn't have all references.");
            return;
        }

        gameState = GameState.PreparingCombat;
        uiManager.SetUIState(gameState);
        currentEncounter = encounter;
        PrepareCombatEncounter();
    }

    void PrepareCombatEncounter()
    {
        // Check player positions and enemy positions, if they are not set, use defaults
        Vector3[] playerTiles = new Vector3[numPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            if (currentEncounter.playerPositions.Length > i)
            {
                playerTiles[i] = currentEncounter.encounterCenter + currentEncounter.playerPositions[i];
            }
            else
            {
                playerTiles[i] = currentEncounter.encounterCenter + defaultPlayerPositions[i];
            }
            //playerMovement.MovePlayerToTile(playerTiles[i]);
        }
        // Set camera to combat position
        orbitCamera.SetCombatCameraPosition(playerTiles[0], currentEncounter.encounterCenter);
        // Move players
        playerMovement.MovePlayerToTile(playerTiles[0], currentEncounter.encounterCenter);
        Debug.Log("Preparing combat: " + currentEncounter.encounterName);
    }

    public void CombatPreparingReady()
    {
        //Vector3 playerTile = currentEncounter.encounterCenter + currentEncounter.playerPositions[0];
        //orbitCamera.SetCombatCameraPosition(playerTile, currentEncounter.encounterCenter + currentEncounter.enemyPositions[0]);
        gameState = GameState.InCombat;
        uiManager.SetUIState(gameState);
        Debug.Log("Started combat: " + currentEncounter.encounterName);
    }

    // Change game and camera state back to free mode
    public void EndCombat()
    {
        gameState = GameState.Free;
        uiManager.SetUIState(gameState);
        if (currentEncounter != null)
        {
            Debug.Log("Ended combat: " + currentEncounter.encounterName);
        }
        currentEncounter = null;
    }
}