using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject tilePrefab;
    public float generationRadius = 10f;
    public float deactivationRadius = 30f;
    public int tilePoolSize = 1000;
    public float zoom = 0.1f;

    [System.Serializable]
    public class TileSpriteData
    {
        public Sprite sprite;
        public float minValue;
        public float maxValue;
    }

    public List<TileSpriteData> tileSprites;

    private List<GameObject> tilePool;
    private Dictionary<Vector3, GameObject> activeTiles;
    private Vector3 lastPlayerTilePosition;
    private Queue<GameObject> inactiveTiles = new Queue<GameObject>();
    private float seed;

    private void Start()
    {
        seed = Random.Range(2500000f, 7500000f);

        tilePool = new List<GameObject>();
        activeTiles = new Dictionary<Vector3, GameObject>();
        lastPlayerTilePosition = Vector3.negativeInfinity;

        for (int i = 0; i < tilePoolSize; i++)
        {
            GameObject newTile = Instantiate(tilePrefab, transform);
            newTile.SetActive(false);
            tilePool.Add(newTile);
            inactiveTiles.Enqueue(newTile);
        }

        GenerateInitialTiles();
        lastPlayerTilePosition = WorldToTilePosition(playerTransform.position);
    }

    private void Update()
    {
        Vector3 currentPlayerTilePosition = WorldToTilePosition(playerTransform.position);

        if (currentPlayerTilePosition != lastPlayerTilePosition)
        {
            GenerateTilesAroundPlayer();
            DeactivateTilesOutsideRadius();
            lastPlayerTilePosition = currentPlayerTilePosition;
        }
    }

    private void GenerateInitialTiles()
    {
        Vector3 currentPlayerTilePosition = WorldToTilePosition(playerTransform.position);
        GenerateTilesAroundPosition(currentPlayerTilePosition, generationRadius);
    }

    private void GenerateTilesAroundPlayer()
    {
        Vector3 currentPlayerTilePosition = WorldToTilePosition(playerTransform.position);
        GenerateTilesAroundPosition(currentPlayerTilePosition, generationRadius);
    }

    private void GenerateTilesAroundPosition(Vector3 position, float radius)
    {
        Vector3 currentPlayerIntPosition = new Vector3(Mathf.Floor(position.x), 0, Mathf.Floor(position.z));

        if (currentPlayerIntPosition != WorldToTilePosition(lastPlayerTilePosition))
        {
            for (int x = (int)currentPlayerIntPosition.x - (int)radius; x <= (int)currentPlayerIntPosition.x + (int)radius; x++)
            {
                for (int z = (int)currentPlayerIntPosition.z - (int)radius; z <= (int)currentPlayerIntPosition.z + (int)radius; z++)
                {
                    Vector3 tilePosition = new Vector3(x, 0, z);

                    if (Vector3.Distance(tilePosition, currentPlayerIntPosition) <= radius)
                    {
                        if (!activeTiles.ContainsKey(tilePosition))
                        {
                            float noiseValue = Mathf.PerlinNoise((tilePosition.x + seed) / zoom, (tilePosition.z + seed) / zoom);

                            Sprite selectedSprite = null;
                            foreach (TileSpriteData spriteData in tileSprites)
                            {
                                if (noiseValue >= spriteData.minValue && noiseValue <= spriteData.maxValue)
                                {
                                    selectedSprite = spriteData.sprite;
                                    break;
                                }
                            }

                            if (selectedSprite != null)
                            {
                                GameObject newTile = GetTileFromPool();
                                PlaceTile(newTile, tilePosition, selectedSprite);
                                activeTiles.Add(tilePosition, newTile);
                            }
                        }
                    }
                }
            }
        }
    }

    private void DeactivateTilesOutsideRadius()
    {
        List<Vector3> tilesToDeactivate = new List<Vector3>();

        foreach (KeyValuePair<Vector3, GameObject> tileEntry in activeTiles)
        {
            Vector3 tilePosition = tileEntry.Key;

            if (Vector3.Distance(tilePosition, playerTransform.position) > deactivationRadius)
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
        {
            return inactiveTiles.Dequeue();
        }
        else
        {
            GameObject newTile = Instantiate(tilePrefab, transform);
            tilePool.Add(newTile);
            return newTile;
        }
    }

    private void ReturnTileToPool(GameObject tile)
    {
        tile.SetActive(false);
        inactiveTiles.Enqueue(tile);
    }

    private void PlaceTile(GameObject tile, Vector3 position, Sprite sprite)
    {
        tile.transform.position = position;
        tile.SetActive(true);
        tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    private Vector3 WorldToTilePosition(Vector3 worldPosition)
    {
        return new Vector3(Mathf.Floor(worldPosition.x), 0f, Mathf.Floor(worldPosition.z));
    }
}
