using UnityEngine;

using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    public Tilemap groundMap;

    public float spawnRadius;
    public float minimumSpawnRadius;
    public float spawnCooldown;
    private float spawnTimer;

    public GameObject[] bushPrefabs;
    public int bushCount;

    private void Start()
    {
        int bushesPlaced = 0;
        while (bushesPlaced < bushCount)
        {
            Vector3Int cellPosition = new Vector3Int
            (
                Random.Range(groundMap.cellBounds.xMin, groundMap.cellBounds.xMax),
                Random.Range(groundMap.cellBounds.yMin, groundMap.cellBounds.yMax),
                0
            );

            if (groundMap.HasTile(cellPosition))
            {
                GameObject bushPrefab = bushPrefabs[Random.Range(0, bushPrefabs.Length)];
                Vector3 spawnPosition = groundMap.GetCellCenterWorld(cellPosition);
                Instantiate(bushPrefab, spawnPosition, Quaternion.Euler(360.0f * Random.value * Vector3.forward), transform);
                bushesPlaced++;
            }
        }
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0.0f)
        {
            if (Spawn())
            {
                spawnTimer += spawnCooldown;
            }
        }
    }

    public bool Spawn()
    {
        if (!PlayerMask.instance) return false;

        // Select random enemy prefab
        Body enemyPrefab = Prefabs.instance.enemyPrefabs[Random.Range(0, Prefabs.instance.enemyPrefabs.Length)];

        // Find random spawn position
        for (int attempts = 0; attempts < 100; attempts++)
        {
            // Get random point around player
            Vector2 point = (Vector2)PlayerMask.instance.transform.position + Random.Range(minimumSpawnRadius, spawnRadius) * Random.insideUnitCircle.normalized;
            Vector3Int cellPosition = groundMap.WorldToCell(point);

            // Check if cell is valid
            if (!groundMap.HasTile(cellPosition)) continue;

            // Instantiate enemy at spawn position
            Vector2 spawnPosition = groundMap.GetCellCenterWorld(cellPosition);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform);

            return true;
        }

        return false;
    }
}
