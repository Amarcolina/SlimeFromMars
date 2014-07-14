using UnityEngine;
using System.Collections;

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
    public const float TILEMAP_SIZE = 10.0f;
    private TileChunk[,] _tilemapChunks = new TileChunk[1, 1];
    private TilemapOffset _chunkOriginOffset = new TilemapOffset(0, 0);


    class TileChunk {
        public const int CHUNK_SIZE = 8;
        private GameObject[,] _tiles = new GameObject[CHUNK_SIZE, CHUNK_SIZE];
        private GameObject _chunkGameObject = null;

        public GameObject gameObject {
            get {
                return _chunkGameObject;
            }
        }

        public TileChunk(GameObject chunkGameObject) {
            _chunkGameObject = chunkGameObject;
        }

        public GameObject getTile(TilemapOffset offset) {
            return _tiles[offset.x, offset.y];
        }

        public void setTile(TilemapOffset offset, GameObject tile) {
            _tiles[offset.x, offset.y] = tile;
        }
    }

    public TilemapOffset getTilemapOffset(Vector2 position) {
        return new TilemapOffset((int)System.Math.Round(position.x / TILEMAP_SIZE, System.MidpointRounding.ToEven),
                                 (int)System.Math.Round(position.y / TILEMAP_SIZE, System.MidpointRounding.ToEven));
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

    public void setTileGameObject(Vector2 position, Tile tile) {
        setTileGameObject(getTilemapOffset(position), tile);
    }

    public void setTileGameObject(TilemapOffset offset, Tile tile) {
        expandTilemapToIncludeOffset(offset);
        TilemapOffset chunkOffset = new TilemapOffset(tileChunkMod(offset.x), tileChunkMod(offset.y));
        TilemapOffset chunkLocation = (offset - chunkOffset) / TileChunk.CHUNK_SIZE;
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        if (tileChunk == null) {
            GameObject newTileChunkGameObject = new GameObject("TileChunk(" + chunkLocation.x + "," + chunkLocation.y + ")");
            newTileChunkGameObject.transform.parent = transform;
            tileChunk = new TileChunk(newTileChunkGameObject);
            _tilemapChunks[chunkLocation.x, chunkLocation.y] = tileChunk;
        }

        GameObject currentTile = tileChunk.getTile(chunkOffset);
        if (currentTile != null) {
            Destroy(currentTile);
        }
        GameObject newTileObject = new GameObject("Tile(" + offset.x + "," + offset.y + ")");
        newTileObject.transform.parent = tileChunk.gameObject.transform;
        tileChunk.setTile(chunkOffset, newTileObject);
    }

    private int tileChunkMod(int x) {
        return (x % TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) % TileChunk.CHUNK_SIZE;
    }

    private void expandTilemapToIncludeOffset(TilemapOffset offset) {
        TilemapOffset transformedOffset = offset - _chunkOriginOffset * 8;
        int increaseLeftX = Mathf.Max(0, -transformedOffset.x);
        int increaseRightX = Mathf.Max(0, transformedOffset.x - _tilemapChunks.GetLength(0) * TileChunk.CHUNK_SIZE);
        int increaseUpY = Mathf.Max(0, -transformedOffset.y);
        int increaseDownY = Mathf.Max(0, transformedOffset.y - _tilemapChunks.GetLength(1) * TileChunk.CHUNK_SIZE);
        int increaseX = Mathf.Max(increaseLeftX, increaseRightX);
        int increaseY = Mathf.Max(increaseUpY, increaseDownY);
        if (increaseX == 0 && increaseY == 0) {
            return;
        }

        TileChunk[,] newChunkArray = new TileChunk[_tilemapChunks.GetLength(0) + increaseX, _tilemapChunks.GetLength(1) + increaseY];

        for (int x = 0; x < _tilemapChunks.GetLength(0); x++) {
            for (int y = 0; y < _tilemapChunks.GetLength(1); y++) {
                newChunkArray[x + increaseLeftX, y + increaseUpY] = _tilemapChunks[x, y];
            }
        }

        _tilemapChunks = newChunkArray;
        _chunkOriginOffset = _chunkOriginOffset + new TilemapOffset(increaseLeftX, increaseUpY);
    }
}
