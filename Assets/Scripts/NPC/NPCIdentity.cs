using System;
using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    [SerializeField] private string npcId;

    public string Id => npcId;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            npcId = Guid.NewGuid().ToString("N");
        }
    }

    [ContextMenu("Generate New NPC ID")]
    private void GenerateNewId()
    {
        npcId = Guid.NewGuid().ToString("N");
    }
}
