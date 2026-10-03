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
    public GameObject collectionPanel;

    private CombatManager combatManager;

    void Awake() => Instance = this;

    void Start()
    {
        combatManager = CombatManager.Instance;
        SetUIState(combatManager.gameState);
        collectionPanel.SetActive(false);
    }

    public void SetUIState(GameState state)
    {
        bool isFreeRoam = state == GameState.Free;
        freeRoamHUD.SetActive(isFreeRoam);
        combatHUD.SetActive(!isFreeRoam);
    }

    //public void SetCollectionUI(bool newState)
    //{
    //    collectionPanel.SetActive(newState);
    //}
}