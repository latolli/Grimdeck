using UnityEngine;

public class DungeonEntrance : MonoBehaviour, IClickable
{
    public Transform InteractionTarget => transform;
    public int InteractionRange => 1;

    public void OnInteract()
    {
        Debug.Log("Entering dungeon " + gameObject.name);
    }
}
