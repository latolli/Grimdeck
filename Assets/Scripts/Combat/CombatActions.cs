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

// Class defining possible actions per one turn
public class CombatAction
{
    CombatActionType[] actionType;          // E.g., [Attack, ApplyDebuff]
    CombatEffectType[] combatEffectType;    // E.e., [None, Weaken]
    int[] actionValue;                      // E.e., [7, 3] (Deal 7 damage with attack, weaken for 3 turns)
}