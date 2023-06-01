using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject tilePrefab;
    public float generationDistance = 10f;
    public float deactivationDistance = 30f;
    public int tilePoolSize = 1000;
    public List<TileLayerData> tileLayers;

    private List<GameObject> tilePool;
    private Dictionary<Vector3, GameObject> activeTiles;
    private Vector3 lastPlayerTilePosition;
    private Queue<GameObject> inactiveTiles = new Queue<GameObject>();

    [System.Serializable]
    public class TileLayerData
    {
        public string layerName;
        [HideInInspector]
        public float seed;
        public float zoom;
        public List<ValueRangeData> valueRanges;
    }

    [System.Serializable]
    public class ValueRangeData
    {
        public float minValue;
        public float maxValue;
        public Sprite sprite;
    }

    private void Start()
    {
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
            DeactivateTilesOutsideDistance();
            lastPlayerTilePosition = currentPlayerTilePosition;
        }
    }

    private void GenerateInitialTiles()
    {
        Vector3 currentPlayerTilePosition = WorldToTilePosition(playerTransform.position);
        GenerateTilesAroundPosition(currentPlayerTilePosition, generationDistance);
    }

    private void GenerateTilesAroundPlayer()
    {
        Vector3 currentPlayerTilePosition = WorldToTilePosition(playerTransform.position);
        GenerateTilesAroundPosition(currentPlayerTilePosition, generationDistance);
    }

    private void GenerateTilesAroundPosition(Vector3 position, float distance)
    {
        Vector3 currentPlayerIntPosition = new Vector3(Mathf.Floor(position.x), 0, Mathf.Floor(position.z));

        if (currentPlayerIntPosition != WorldToTilePosition(lastPlayerTilePosition))
        {
            float horizontalDistance = distance * 2;
            float verticalDistance = distance;

            foreach (TileLayerData layerData in tileLayers)
            {
                if (layerData.seed == 0f)
                {
                    layerData.seed = Random.Range(2500000f, 7500000f);
                }

                for (int x = (int)currentPlayerIntPosition.x - (int)horizontalDistance; x <= (int)currentPlayerIntPosition.x + (int)horizontalDistance; x++)
                {
                    for (int z = (int)currentPlayerIntPosition.z - (int)verticalDistance; z <= (int)currentPlayerIntPosition.z + (int)verticalDistance; z++)
                    {
                        Vector3 tilePosition = new Vector3(x, 0, z);

                        if (!activeTiles.ContainsKey(tilePosition))
                        {
                            float noiseValue = Mathf.PerlinNoise((tilePosition.x + layerData.seed) / layerData.zoom, (tilePosition.z + layerData.seed) / layerData.zoom);
                            Sprite selectedSprite = null;

                            foreach (ValueRangeData valueRangeData in layerData.valueRanges)
                            {
                                if (noiseValue >= valueRangeData.minValue && noiseValue <= valueRangeData.maxValue)
                                {
                                    selectedSprite = valueRangeData.sprite;
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

    private void DeactivateTilesOutsideDistance()
    {
        float horizontalDistance = deactivationDistance * 2;
        float verticalDistance = deactivationDistance;

        List<Vector3> tilesToDeactivate = new List<Vector3>();

        foreach (KeyValuePair<Vector3, GameObject> tileEntry in activeTiles)
        {
            Vector3 tilePosition = tileEntry.Key;

            if (Mathf.Abs(tilePosition.x - playerTransform.position.x) > horizontalDistance ||
                Mathf.Abs(tilePosition.z - playerTransform.position.z) > verticalDistance)
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

        // Set the sprite
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    private Vector3 WorldToTilePosition(Vector3 worldPosition)
    {
        return new Vector3(Mathf.Floor(worldPosition.x), 0f, Mathf.Floor(worldPosition.z));
    }
}
