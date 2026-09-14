using UnityEngine;

[RequireComponent(typeof(NPCIdentity))]
public class CombatEnemy : MonoBehaviour, ICombatStats
{
    [SerializeField, Min(1)] private int maxHp = 1;
    [SerializeField, Min(0)] private int currentHp;
    [SerializeField] private CombatAction[] actions = System.Array.Empty<CombatAction>();

    private NPCIdentity npcIdentity;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public bool IsDead => currentHp <= 0;
    public CombatAction[] Actions => actions;

    private void Awake()
    {
        npcIdentity = GetComponent<NPCIdentity>();
        currentHp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError($"Cannot deal negative damage to {name}.", this);
            return;
        }

        currentHp = Mathf.Max(0, currentHp - amount);
    }

    public void ResetCombatState()
    {
        currentHp = maxHp;
    }

    private void OnValidate()
    {
        maxHp = Mathf.Max(1, maxHp);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
    }
}