using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public class Tilemap : MonoBehaviour {
    public const float TILE_SIZE = 1.0f;
    [SerializeField]
    [HideInInspector]
    private Array2DTC _tilemapChunks = null;
    [SerializeField]
    [HideInInspector]
    public Vector2Int _chunkOriginOffset = new Vector2Int(0, 0);

#if UNITY_EDITOR
    public static void recalculateTileImages() {
        TextureCombiner.clearCachedSprites();
        HashSet<GameObject> tilePrefabs = new HashSet<GameObject>();
        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles) {
            GameObject prefab = (GameObject)PrefabUtility.GetPrefabParent(tile.gameObject);
            if (prefab) {
                if (!tilePrefabs.Contains(prefab)) {
                    tilePrefabs.Add(prefab);
                    prefab.GetComponent<Tile>().updateTileWithSettings();
                }
            }
        }
    }
#endif

    public void clear() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child && child != transform) {
                DestroyImmediate(child.gameObject);
            }
        }
        _tilemapChunks = ScriptableObject.CreateInstance<Array2DTC>();
        _tilemapChunks.init(1, 1);
        _chunkOriginOffset = new Vector2Int(0, 0);
    }

    public static Vector2Int getTilemapLocation(Vector2 position) {
        return new Vector2Int((int)System.Math.Round(position.x / TILE_SIZE, System.MidpointRounding.ToEven),
                                 (int)System.Math.Round(position.y / TILE_SIZE, System.MidpointRounding.ToEven));
    }

    public Tile getTile(Vector2 position) {
        return getTile(getTilemapLocation(position));
    }

    public Tile getTile(Vector2Int location) {
        return getTileGameObject(location).GetComponent<Tile>();
    }

    public GameObject getTileGameObject(Vector2 position) {
        return getTileGameObject(getTilemapLocation(position));
    }

    public GameObject getTileGameObject(Vector2Int tileLocation) {
        Vector2Int tileInChunkLocation = new Vector2Int(tileChunkMod(tileLocation.x), tileChunkMod(tileLocation.y));
        Vector2Int chunkLocation = (tileLocation - tileInChunkLocation) / TileChunk.CHUNK_SIZE;
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        if (tileChunk == null) {
            return null;
        }

        return tileChunk.getTile(tileInChunkLocation);
    }

#if UNITY_EDITOR
    public void setTilePrefab(Vector2Int tileLocation, GameObject prefab) {
        expandTilemapToIncludeLocation(tileLocation);
        Vector2Int tileInChunkLocation = new Vector2Int(tileChunkMod(tileLocation.x), tileChunkMod(tileLocation.y));
        Vector2Int chunkLocation = (tileLocation - tileInChunkLocation - _chunkOriginOffset * TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        if (tileChunk == null) {
            GameObject newTileChunkGameObject = new GameObject("TileChunk(" + chunkLocation.x + "," + chunkLocation.y + ")");
            newTileChunkGameObject.transform.parent = transform;
            tileChunk = ScriptableObject.CreateInstance<TileChunk>();
            tileChunk.init(newTileChunkGameObject); 
            _tilemapChunks[chunkLocation.x, chunkLocation.y] = tileChunk;
        }

        GameObject currentTile = tileChunk.getTile(tileInChunkLocation);
        if (currentTile != null) {
            DestroyImmediate(currentTile);
        }

        GameObject newTileObject = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
        newTileObject.name += "(" + tileLocation.x + "," + tileLocation.y + ")";
        newTileObject.transform.parent = tileChunk.gameObject.transform;
        newTileObject.transform.position = new Vector3(tileLocation.x, tileLocation.y, 0) * TILE_SIZE;
        newTileObject.GetComponent<Tile>().updateTileWithSettings();
        tileChunk.setTile(tileInChunkLocation, newTileObject);
    }
#endif

    private int tileChunkMod(int x) {
        return (x % TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) % TileChunk.CHUNK_SIZE;
    }

    private void expandTilemapToIncludeLocation(Vector2Int tileLocation) {
        Vector2Int tilemapArrayIndex = tileLocation - _chunkOriginOffset * 8;
        int increaseLeftX = Mathf.Max(0, -tilemapArrayIndex.x + TileChunk.CHUNK_SIZE - 1) / TileChunk.CHUNK_SIZE;
        int increaseRightX = Mathf.Max(0, tilemapArrayIndex.x - _tilemapChunks.width * TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        int increaseUpY = Mathf.Max(0, -tilemapArrayIndex.y + TileChunk.CHUNK_SIZE - 1) / TileChunk.CHUNK_SIZE;
        int increaseDownY = Mathf.Max(0, tilemapArrayIndex.y - _tilemapChunks.height * TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        int increaseX = Mathf.Max(increaseLeftX, increaseRightX);
        int increaseY = Mathf.Max(increaseUpY, increaseDownY);

        if (increaseX == 0 && increaseY == 0) {
            return;
        }

        Array2DTC newChunkArray = ScriptableObject.CreateInstance<Array2DTC>();
        newChunkArray.init(_tilemapChunks.width + increaseX, _tilemapChunks.height + increaseY);

        for (int x = 0; x < _tilemapChunks.width; x++) {
            for (int y = 0; y < _tilemapChunks.height; y++) {
                newChunkArray[x + increaseLeftX, y + increaseUpY] = _tilemapChunks[x, y];
            }
        }

        _tilemapChunks = newChunkArray;
        _chunkOriginOffset = _chunkOriginOffset - new Vector2Int(increaseLeftX, increaseUpY);
    }

    public void OnDrawGizmos() {
        if (_tilemapChunks == null) {
            clear();
        }
        for (int x = 0; x < _tilemapChunks.width; x++) {
            for (int y = 0; y < _tilemapChunks.height; y++) {
                Vector2 cRight = Vector2.right * TileChunk.CHUNK_SIZE * TILE_SIZE;
                Vector2 cUp = Vector2.up * TileChunk.CHUNK_SIZE * TILE_SIZE;
                Vector2 v = new Vector2(x + _chunkOriginOffset.x, y + _chunkOriginOffset.y) * TileChunk.CHUNK_SIZE * TILE_SIZE - Vector2.one * TILE_SIZE / 2.0f;

                TileChunk chunk = _tilemapChunks[x, y];
                if (chunk != null) {
                    for (int dx = 0; dx < TileChunk.CHUNK_SIZE; dx++) {
                        for (int dy = 0; dy < TileChunk.CHUNK_SIZE; dy++) {
                            GameObject obj = chunk.getTile(new Vector2Int(dx, dy));
                            if (obj == null) {
                                Vector2 v2 = v + new Vector2(dx, dy) * TILE_SIZE;
                                Vector2 tRight = Vector2.right * TILE_SIZE;
                                Vector2 tUp = Vector2.up * TILE_SIZE;
                                Gizmos.color = new Color(0.2f, 0.2f, 0.2f);
                                Gizmos.DrawLine(v2, v2 + tRight);
                                Gizmos.DrawLine(v2, v2 + tUp);
                                Gizmos.DrawLine(v2 + tRight, v2 + tRight + tUp);
                                Gizmos.DrawLine(v2 + tUp, v2 + tRight + tUp);
                            }
                        }
                    }
                }

                Gizmos.color = Color.black;
                Gizmos.DrawLine(v, v + cRight);
                Gizmos.DrawLine(v, v + cUp);
                Gizmos.DrawLine(v + cRight, v + cRight + cUp);
                Gizmos.DrawLine(v + cUp, v + cRight + cUp);
            }
        }
    }
}
