using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Combats/CombatEncounter")]
public class CombatEncounter : ScriptableObject
{
    public string encounterName;
    public string encounterDescription;
    public string[] enemyIds;
    public Vector3 encounterCenter;             // Center of the encounter area
    public Vector3[] enemyPositions;            // If 0, use defaults
    public Vector3[] playerPositions;           // If 0, use defaults
}