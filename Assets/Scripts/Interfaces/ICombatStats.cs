
public enum CombatActionType
{
    Attack,
    Block,
    Heal,
    ApplyDebuff
}

public enum CombatEffectType
{
    None,
    Poison,
    Stun,
    Weaken
}

[System.Serializable]
public class CombatAction
{
    public CombatActionType type;
    public int value;
    public CombatEffectType effect;
    public int effectDuration;
}

public interface ICombatStats
{
    int MaxHp { get; }
    int CurrentHp { get; }
    bool IsDead { get; }
    CombatAction[] Actions { get; }
    void TakeDamage(int amount);
}