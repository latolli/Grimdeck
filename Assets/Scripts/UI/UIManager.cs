using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject inventoryPanel;
    public GameObject statsPanel;
    public GameObject drawPilePanel;
    public GameObject discardPilePanel;
    public GameObject freeRoamHUD;
    public GameObject combatHUD;

    private CombatManager combatManager;

    void Awake() => Instance = this;

    void Start()
    {
        combatManager = CombatManager.Instance;
        SetUIState(combatManager.gameState);
    }

    public void SetUIState(GameState state)
    {
        bool isFreeRoam = state == GameState.Free;
        freeRoamHUD.SetActive(isFreeRoam);
        combatHUD.SetActive(!isFreeRoam);
    }
}