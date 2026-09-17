public enum CombatActionType
{
    Attack,
    Block,
    Heal,
    ApplyDebuff,
    Draw,       // Only for player
    Discard,    // Only for player
    Destroy,    // Only for player
}

public enum CombatEffectType
{
    None,
    Poison,
    Fire,
    Weaken
}

// Class defining possible actions per one turn
public class CombatAction
{
    public CombatActionType[] ActionTypes { get; set; }
    public CombatEffectType[] CombatEffectTypes { get; set; }
    public int[] ActionValues { get; set; }

    public CombatAction()
        : this(
            new CombatActionType[0],
            new CombatEffectType[0],
            new int[0])
    {
    }

    public CombatAction(
        CombatActionType[] actionTypes,
        CombatEffectType[] combatEffectTypes,
        int[] actionValues)
    {
        if (actionTypes == null ||
            combatEffectTypes == null ||
            actionValues == null ||
            actionTypes.Length != combatEffectTypes.Length ||
            actionTypes.Length != actionValues.Length)
        {
            throw new System.ArgumentException(
                "Combat action types, effects, and values must have the same length.");
        }

        ActionTypes = actionTypes;
        CombatEffectTypes = combatEffectTypes;
        ActionValues = actionValues;
    }
}

// This class will be used to track how many turns left of each status
public class EffectStatus
{
    public int Weakened;
    public int Poisoned;
    public int OnFire;
}