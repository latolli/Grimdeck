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
    private CombatActions combatActions;
    private OrbitCamera orbitCamera;
    public int numPlayers = 1;  // Hardcoded for now, can be set dynamically later

    void Awake() => Instance = this;

    void Start()
    {
        uiManager = UIManager.Instance;
        orbitCamera = FindFirstObjectByType<OrbitCamera>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        combatActions = FindFirstObjectByType<CombatActions>();
        combatActions.enabled = false;      // Combat actions will be enabled when entering combat
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
        Vector3[] defaultPositions = new Vector3[numPlayers + currentEncounter.enemyIds.Length];
        Vector3 firstPlayerPos = currentEncounter.relativePlayerPositions[0];
        if (firstPlayerPos == null)
        {
            Debug.LogError("At least one player position needed");
            return;
        }
        // Check rest of player positions
        for (int i = 1; i < numPlayers; i++)
        {
            // First, add any x or z offset that first player has to center
            Vector3 newPlayerPos = firstPlayerPos + firstPlayerPos;
            // Then, add offsets in either X or Z axis depending on the orientation
            Vector3 offsetVector = firstPlayerPos.x == 0 ? new Vector3(2,0,0) : new Vector3(0,0,2);
            if (i % 2 == 0)
            {
                newPlayerPos += offsetVector;
            }
            else
            {
                newPlayerPos -= offsetVector;
            }
            defaultPositions[i] = newPlayerPos;
            Debug.Log("Combat position:" + i + newPlayerPos);
        }

        for (int i = 0; i < numPlayers; i++)
        {
            if (currentEncounter.relativePlayerPositions.Length > i)
            {
                playerTiles[i] = currentEncounter.encounterCenter + currentEncounter.relativePlayerPositions[i];
            }
            else
            {
                playerTiles[i] = currentEncounter.encounterCenter + defaultPositions[i];
            }
            //playerMovement.MovePlayerToTile(playerTiles[i]);
        }
        // Set camera to combat position
        orbitCamera.SetCombatCameraPosition(firstPlayerPos, currentEncounter.encounterCenter);
        // Move players
        playerMovement.MovePlayerToTile(playerTiles[0], currentEncounter.encounterCenter);
        Debug.Log("Preparing combat: " + currentEncounter.encounterName);
    }

    public void CombatPreparingReady()
    {
        // Update states and enable / disable needed components
        gameState = GameState.InCombat;
        uiManager.SetUIState(gameState);
        playerMovement.enabled = false;
        combatActions.enabled = true;
        Debug.Log("Started combat: " + currentEncounter.encounterName);
    }

    // Change game and camera state back to free mode
    public void EndCombat()
    {
        // Update states and enable / disable needed components
        gameState = GameState.Free;
        uiManager.SetUIState(gameState);
        playerMovement.enabled = true;
        combatActions.enabled = false;
        if (currentEncounter != null)
        {
            Debug.Log("Ended combat: " + currentEncounter.encounterName);
        }
        currentEncounter = null;
    }
}