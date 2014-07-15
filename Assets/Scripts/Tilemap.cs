using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public struct TilemapOffset {
    public readonly int x, y;

    public TilemapOffset(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public static TilemapOffset operator +(TilemapOffset a, TilemapOffset b) {
        return new TilemapOffset(a.x + b.x, a.y + b.y);
    }

    public static TilemapOffset operator -(TilemapOffset a, TilemapOffset b) {
        return new TilemapOffset(a.x - b.x, a.y - b.y);
    }

    public static TilemapOffset operator *(TilemapOffset a, int x) {
        return new TilemapOffset(a.x * x, a.y * x);
    }

    public static TilemapOffset operator /(TilemapOffset a, int d) {
        return new TilemapOffset(a.x / d, a.y / d);
    }
}



public class Tilemap : MonoBehaviour {
    public const float TILE_SIZE = 1.0f;
    public Array2DTC _tilemapChunks = null;
    public TilemapOffset _chunkOriginOffset = new TilemapOffset(0, 0);

    [System.Serializable]
    public class TileChunk : ScriptableObject{
        public const int CHUNK_SIZE = 8;
        public Array2DGO _tiles = null;
        public GameObject _chunkGameObject = null;

        public GameObject gameObject {
            get {
                return _chunkGameObject;
            }
        }

        public void init(GameObject chunkGameObject) {
            _tiles = ScriptableObject.CreateInstance<Array2DGO>();
            _tiles.init(CHUNK_SIZE, CHUNK_SIZE);
            _chunkGameObject = chunkGameObject;
        }

        public GameObject getTile(TilemapOffset offset) {
            return _tiles[offset.x, offset.y];
        }

        public void setTile(TilemapOffset offset, GameObject tile) {
            _tiles[offset.x, offset.y] = tile;
        }
    }

    public void clear() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child && child != transform) {
                DestroyImmediate(child.gameObject);
            }
        }
        _tilemapChunks = ScriptableObject.CreateInstance<Array2DTC>();
        _tilemapChunks.init(1, 1);
        _chunkOriginOffset = new TilemapOffset(0, 0);
    }

    public static TilemapOffset getTilemapOffset(Vector2 position) {
        return new TilemapOffset((int)System.Math.Round(position.x / TILE_SIZE, System.MidpointRounding.ToEven),
                                 (int)System.Math.Round(position.y / TILE_SIZE, System.MidpointRounding.ToEven));
    }

    public Tile getTile(Vector2 position) {
        return getTile(getTilemapOffset(position));
    }

    public Tile getTile(TilemapOffset offset) {
        return getTileGameObject(offset).GetComponent<Tile>();
    }

    public GameObject getTileGameObject(Vector2 position) {
        return getTileGameObject(getTilemapOffset(position));
    }

    public GameObject getTileGameObject(TilemapOffset offset) {
        TilemapOffset chunkOffset = new TilemapOffset(tileChunkMod(offset.x), tileChunkMod(offset.y));
        TilemapOffset chunkLocation = (offset - chunkOffset) / TileChunk.CHUNK_SIZE;
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        if (tileChunk == null) {
            return null;
        }

        return tileChunk.getTile(chunkOffset);
    }

#if UNITY_EDITOR
    public void setTilePrefab(TilemapOffset offset, GameObject prefab) {
        expandTilemapToIncludeOffset(offset);
        TilemapOffset chunkOffset = new TilemapOffset(tileChunkMod(offset.x), tileChunkMod(offset.y));
        TilemapOffset chunkLocation = (offset - chunkOffset - _chunkOriginOffset * TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        if (tileChunk == null) {
            GameObject newTileChunkGameObject = new GameObject("TileChunk(" + chunkLocation.x + "," + chunkLocation.y + ")");
            newTileChunkGameObject.transform.parent = transform;
            tileChunk = ScriptableObject.CreateInstance<TileChunk>();
            tileChunk.init(newTileChunkGameObject); 
            _tilemapChunks[chunkLocation.x, chunkLocation.y] = tileChunk;
        }

        GameObject currentTile = tileChunk.getTile(chunkOffset);
        if (currentTile != null) {
            DestroyImmediate(currentTile);
        }

        GameObject newTileObject = (GameObject) PrefabUtility.InstantiatePrefab(prefab);     
        newTileObject.name = "Tile(" + offset.x + "," + offset.y + ")";
        newTileObject.transform.parent = tileChunk.gameObject.transform;
        newTileObject.transform.position = new Vector3(offset.x, offset.y, 0) * TILE_SIZE;
        tileChunk.setTile(chunkOffset, newTileObject);
    }
#endif

    private int tileChunkMod(int x) {
        return (x % TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) % TileChunk.CHUNK_SIZE;
    }

    private void expandTilemapToIncludeOffset(TilemapOffset offset) {
        TilemapOffset transformedOffset = offset - _chunkOriginOffset * 8;
        int increaseLeftX = Mathf.Max(0, -transformedOffset.x + TileChunk.CHUNK_SIZE - 1) / TileChunk.CHUNK_SIZE;
        int increaseRightX = Mathf.Max(0, transformedOffset.x - _tilemapChunks.width * TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        int increaseUpY = Mathf.Max(0, -transformedOffset.y + TileChunk.CHUNK_SIZE - 1) / TileChunk.CHUNK_SIZE;
        int increaseDownY = Mathf.Max(0, transformedOffset.y - _tilemapChunks.height * TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
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
        _chunkOriginOffset = _chunkOriginOffset - new TilemapOffset(increaseLeftX, increaseUpY);
    }

    public void OnDrawGizmos() {
        if (_tilemapChunks == null) {
            Debug.Log("remaking it");
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
                            GameObject obj = chunk.getTile(new TilemapOffset(dx, dy));
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
