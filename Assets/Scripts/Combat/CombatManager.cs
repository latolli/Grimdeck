using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Free,
    InCombat,
    PreparingCombat
}

public enum CombatState
{
    None,
    PlayerTurn,
    EnemyTurn,
    EndScreen
}

public enum CombatResult
{
    Victory,
    Defeat,
    Escape
}

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatStatsPanel[] enemyStats = new CombatStatsPanel[3];
    [SerializeField] private CombatStatsPanel[] playerStats = new CombatStatsPanel[3];
    public static CombatManager Instance;
    public GameState gameState = GameState.Free;
    public CombatState combatState = CombatState.None;
    UIManager uiManager;
    public CombatCatalog combatCatalog;
    private CombatEncounter currentEncounter;
    private PlayerMovement playerMovement;
    private PlayerCombatHandler playerCombatHandler;
    private CardManager cardManager;
    private OrbitCamera orbitCamera;
    private List<string> aliveEnemies;
    public int numPlayers = 1;  // Hardcoded for now, can be set dynamically later
    private int playerTurnCounter;
    private int enemyTurnCounter;

    void Awake() => Instance = this;

    void Start()
    {
        uiManager = UIManager.Instance;
        orbitCamera = FindFirstObjectByType<OrbitCamera>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerCombatHandler = FindFirstObjectByType<PlayerCombatHandler>();
        cardManager = FindFirstObjectByType<CardManager>();
        playerCombatHandler.enabled = false;      // Combat actions will be enabled when entering combat
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
    }

    void ChangeEnemyStates(bool start, string[] enemyList)
    {
        for (int i = 0; i < enemyList.Length; i++)
        {
            string id = enemyList[i];
            if (!NPCRegistry.Instance.TryGetNPC(id, out NPCIdentity npc))
            {
                Debug.LogError($"Enemy with ID '{id}' was not found in the NPC registry.");
                continue;
            }

            RegularEnemy enemy = npc.GetComponent<RegularEnemy>();
            if (enemy == null)
            {
                Debug.LogError($"NPC with ID '{id}' is not a RegularEnemy.", npc);
                continue;
            }

            if (start)
            {
                enemy.ResetEnemyCombatState();
            }
            else
            {
                enemy.NullifyCombatState();
            }
        }

        // Initialize list of currently alive enemies
        if (start)
        {
            aliveEnemies = new List<string>(enemyList);
        }
    }

    // Callbacks
    public void CombatPreparingReadyCB()
    {
        // Update states and enable / disable needed components
        gameState = GameState.InCombat;
        uiManager.SetUIState(gameState);
        playerMovement.enabled = false;
        playerCombatHandler.enabled = true;
        playerTurnCounter = 0;
        enemyTurnCounter = 0;

        // Reset enemy states
        if (NPCRegistry.Instance == null)
        {
            Debug.LogError("NPCRegistry not found in the scene.");
            return;
        }

        // Prepare enemy states and cards
        ChangeEnemyStates(true, currentEncounter.enemyIds);
        playerCombatHandler.ResetPlayerCombatState();
        cardManager.PrepareCardsForCombat(true);

        // Start first player turn
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        // Start player turn
        combatState = CombatState.PlayerTurn;
        bool playerAlive = playerCombatHandler.TurnStartEffects();
        if (playerAlive)
        {
            cardManager.StartTurnActions();
        }
        else
        {
            // Player died to poison or something...
            EndCombatCB(CombatResult.Defeat);
        }
    }

    public void EndPlayerTurnCB()
    {
        // Ends player turn and discard remaining hand
        combatState = CombatState.EnemyTurn;
        cardManager.DiscardCards(99);
        playerTurnCounter++;

        // Play enemy turns
        for (int i = 0; i < currentEncounter.enemyIds.Length; i++)
        {
            string id = currentEncounter.enemyIds[i];
            NPCRegistry.Instance.TryGetNPC(id, out NPCIdentity npc);
            RegularEnemy enemy = npc.GetComponent<RegularEnemy>();
            enemy.PlayEnemyTurn(enemyTurnCounter);
            enemyTurnCounter++;
        }

        // End combat if all enemies died during their turn
        if (aliveEnemies.Count == 0)
        {
            EndCombatCB(CombatResult.Victory);
        }
        else
        {     
            // Start player turn again
            StartPlayerTurn();
        }
    }

    // Function to handle enemy's action towards player
    public void EnemyActionCB(CombatActionType action, CombatEffectType effect, int value)
    {
        playerCombatHandler.ApplyActionToPlayer(action, effect, value);
    }

    // Change game and camera state back to free mode
    public void EndCombatCB(CombatResult result)
    {
        // Check combat result
        if (result == CombatResult.Victory)
        {
            Debug.Log("Player won!!");
        }
        else if (result == CombatResult.Defeat)
        {
            Debug.Log("Player lost!!");
        }
        else
        {
            Debug.Log("Player was noob and run away...");
        }

        // Update states and enable / disable needed components
        for (int i = 0; i < currentEncounter.enemyIds.Length; i++)
        {
            enemyStats[i].ResetStats();
            playerStats[i].ResetStats();
        }
        combatState = CombatState.None;
        gameState = GameState.Free;
        cardManager.PrepareCardsForCombat(false);
        uiManager.SetUIState(gameState);
        playerMovement.enabled = true;
        playerCombatHandler.enabled = false;
        //playerTurnCounter = 0;
        //enemyTurnCounter = 0;
        ChangeEnemyStates(false, currentEncounter.enemyIds);
        currentEncounter = null;
    }

    public void EnemyKilledCB(string npcId)
    {
        aliveEnemies.Remove(npcId);
        Debug.Log($"Enemy '{npcId}' defeated. {aliveEnemies.Count} enemies remaining.", this);
        // Only end combat here if enemy died from player attack
        if (aliveEnemies.Count == 0 && combatState == CombatState.PlayerTurn)
        {
            EndCombatCB(CombatResult.Victory);
        }
    }

    // Give empty string or just id (0,1, or 2) for players
    public void UpdateStatsPanelCB(string npcId, CombatStats updatedStats, bool isEnemy)
    {
        if (isEnemy)
        {
            for (int i = 0; i < currentEncounter.enemyIds.Length; i++)
            {
                string id = currentEncounter.enemyIds[i];
                if (npcId == id)
                {
                    enemyStats[i].UpdateStats(updatedStats);
                }
            }
        }
        else
        {
            //int playerIndex = (int) npcId;    Something like this in future
            playerStats[0].UpdateStats(updatedStats);
        }
    }
}