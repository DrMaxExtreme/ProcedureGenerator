using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Transform playerTransform;
    public List<GameObject> tilePrefabs;
    public float generationRadius = 10f;
    public float deactivationRadius = 30f;
    public int tilePoolSize = 1000;
    public float tileSize = 25f;

    private List<GameObject> tilePool;
    private Dictionary<Vector3, GameObject> activeTiles;
    private Queue<GameObject> inactiveTiles = new Queue<GameObject>();

    private void Start()
    {
        tilePool = new List<GameObject>(tilePoolSize);
        activeTiles = new Dictionary<Vector3, GameObject>(tilePoolSize);

        for (int i = 0; i < tilePoolSize; i++)
        {
            GameObject newTile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)], transform);
            newTile.SetActive(false);
            tilePool.Add(newTile);
            inactiveTiles.Enqueue(newTile);
        }

        GenerateTiles();
    }

    private void Update()
    {
        GenerateTiles();
        DeactivateTiles();
    }

    private void GenerateTiles()
    {
        Vector3 currentPlayerTilePosition = WorldToTilePosition(playerTransform.position);
        int horizontalTiles = Mathf.CeilToInt(generationRadius / tileSize);
        int verticalTiles = Mathf.CeilToInt(generationRadius / tileSize);

        for (int x = -horizontalTiles; x <= horizontalTiles; x++)
        {
            for (int z = -verticalTiles; z <= verticalTiles; z++)
            {
                Vector3 tilePosition = currentPlayerTilePosition + new Vector3(x * tileSize, 0, z * tileSize);

                if (!activeTiles.ContainsKey(tilePosition))
                {
                    GameObject newTile = GetTileFromPool();
                    PlaceTile(newTile, tilePosition);
                    activeTiles.Add(tilePosition, newTile);
                }
            }
        }
    }

    private void DeactivateTiles()
    {
        int horizontalTiles = Mathf.CeilToInt(deactivationRadius / tileSize);
        int verticalTiles = Mathf.CeilToInt(deactivationRadius / tileSize);

        List<Vector3> tilesToDeactivate = new List<Vector3>();

        foreach (KeyValuePair<Vector3, GameObject> tileEntry in activeTiles)
        {
            Vector3 tilePosition = tileEntry.Key;

            if (Mathf.Abs(tilePosition.x - playerTransform.position.x) > horizontalTiles * tileSize ||
                Mathf.Abs(tilePosition.z - playerTransform.position.z) > verticalTiles * tileSize)
            {
                tilesToDeactivate.Add(tilePosition);
            }
        }

        foreach (Vector3 tilePosition in tilesToDeactivate)
        {
            GameObject tileToDeactivate = activeTiles[tilePosition];
            activeTiles.Remove(tilePosition);
            ReturnTileToPool(tileToDeactivate);
        }
    }

    private GameObject GetTileFromPool()
    {
        if (inactiveTiles.Count > 0)
            return inactiveTiles.Dequeue();

        GameObject newTile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)], transform);
        tilePool.Add(newTile);
        return newTile;
    }

    private void ReturnTileToPool(GameObject tile)
    {
        tile.SetActive(false);
        inactiveTiles.Enqueue(tile);
    }

    private void PlaceTile(GameObject tile, Vector3 position)
    {
        tile.transform.position = position;
        tile.SetActive(true);
        tile.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f);
    }

    private Vector3 WorldToTilePosition(Vector3 worldPosition)
    {
        return new Vector3(
            Mathf.Floor(worldPosition.x / tileSize) * tileSize,
            0f,
            Mathf.Floor(worldPosition.z / tileSize) * tileSize
        );
    }
}
