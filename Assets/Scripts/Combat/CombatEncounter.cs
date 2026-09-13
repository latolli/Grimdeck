using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Combats/CombatEncounter")]
public class CombatEncounter : ScriptableObject
{
    public string encounterName;
    public string encounterDescription;
    public string[] enemyIds;
    public Vector3 encounterCenter;             // Center of the encounter area
    public Vector3[] relativeEnemyPositions;            // If 0, use defaults
    public Vector3[] relativePlayerPositions;           // If 0, use defaults. But at least first player needs position!
}