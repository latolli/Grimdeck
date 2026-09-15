
using UnityEngine;

public interface IEnemy
{
    Transform InteractionTarget { get; }   // where to walk to (usually its own transform)
    int InteractionRange { get; }           // how close counts as "arrived". Counted as tiles, not world units. 1 = adjacent tile, 0 = same tile.
    void OnInteract();
    void OnCardTarget(CombatActionType action, CombatEffectType effect, int value);
}