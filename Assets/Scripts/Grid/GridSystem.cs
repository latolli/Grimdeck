using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public float tileSize = 1f;

    public static GridSystem Instance;
    void Awake()
    {
        Instance = this;
    }

    // World position -> grid coordinate
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.z / tileSize); // or .y for 2D
        return new Vector2Int(x, y);
    }

    // Grid coordinate -> world position (center of tile)
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = gridPos.x * tileSize + tileSize / 2f;
        float z = gridPos.y * tileSize + tileSize / 2f;
        return new Vector3(x, 0f, z);
    }

    // Combine function for just moving
    public Vector3 SnapToTileCenter(Vector3 worldPos)
    {
        return GridToWorld(WorldToGrid(worldPos));
    }

    // Get all tiles within a certain range of a world position
    public List<Vector3> GetTilesInRange(Vector3 targetPos, int range)
    {
        List<Vector3> tiles = new List<Vector3>();
        Vector2Int gridPos = WorldToGrid(targetPos);

        for (int x = gridPos.x - range; x <= gridPos.x + range; x++)
        {
            for (int y = gridPos.y - range; y <= gridPos.y + range; y++)
            {
                Vector3 tilePos = GridToWorld(new Vector2Int(x, y));
                tiles.Add(tilePos);
            }
        }

        return tiles;
    }
}
