using System;
using UnityEngine;

public class QuestItemIdentity : MonoBehaviour
{
    [SerializeField] private string targetId;

    public string Id => targetId;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            targetId = Guid.NewGuid().ToString("N");
        }
    }

    [ContextMenu("Generate New Target ID")]
    private void GenerateNewId()
    {
        targetId = Guid.NewGuid().ToString("N");
    }
}