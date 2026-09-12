using System.Collections.Generic;
using UnityEngine;

public class NPCRegistry : MonoBehaviour
{
    public static NPCRegistry Instance { get; private set; }

    private readonly Dictionary<string, NPCIdentity> npcsById = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple NPCRegistry instances exist in this scene.", this);
            return;
        }

        Instance = this;
        BuildRegistry();
    }

    private void BuildRegistry()
    {
        npcsById.Clear();

        NPCIdentity[] identities = FindObjectsByType<NPCIdentity>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (NPCIdentity identity in identities)
        {
            if (string.IsNullOrWhiteSpace(identity.Id))
            {
                Debug.LogError($"NPC '{identity.name}' has no NPC ID.", identity);
                continue;
            }

            if (npcsById.ContainsKey(identity.Id))
            {
                Debug.LogError($"Duplicate NPC ID '{identity.Id}'.", identity);
                continue;
            }

            npcsById.Add(identity.Id, identity);
        }
    }

    public bool TryGetNPC(string npcId, out NPCIdentity npc)
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            npc = null;
            return false;
        }

        return npcsById.TryGetValue(npcId, out npc);
    }
}
