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

    /* Clears the tilemap of all tiles, and sets up all internal
     * variables to contain an empty tilemap.  This properly destroys
     * all internal variables and external tiles
     */
    public void clearTilemap() {
        Undo.RecordObject(this, "Cleared tilemap");
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child && child != transform) {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        if (_tilemapChunks) {
            for (int i = 0; i < _tilemapChunks.width; i++) {
                for (int j = 0; j < _tilemapChunks.height; j++) {
                    if (_tilemapChunks[i, j]) {
                        Undo.DestroyObjectImmediate(_tilemapChunks[i, j]);
                    }
                }
            }
            Undo.DestroyObjectImmediate(_tilemapChunks);
        }

        _tilemapChunks = ScriptableObject.CreateInstance<Array2DTC>();
        Undo.RegisterCreatedObjectUndo(_tilemapChunks, "Created new default tilemap chunk array");
        _tilemapChunks.init(1, 1);
        _chunkOriginOffset = new Vector2Int(0, 0);
    }

    /* Given a 2D world position, return the Integer position on 
     * the tilemap.
     */
    public static Vector2Int getTilemapLocation(Vector2 position) {
        return new Vector2Int((int)System.Math.Round(position.x / TILE_SIZE, System.MidpointRounding.ToEven),
                                 (int)System.Math.Round(position.y / TILE_SIZE, System.MidpointRounding.ToEven));
    }

    //####################################################################################################
    /* Given a 2D world position, return the tile that is located
     * at that position.  This returns the Tile component, not
     * the GameObject itself. If there is no Tile located at that
     * position, null is returned.
     */
    public Tile getTile(Vector2Int location) {
        GameObject tileObj = getTileGameObject(location);
        if (tileObj == null) {
            return null;
        }
        return tileObj.GetComponent<Tile>();
    }

    public Tile getTile(Vector2 position) {
        return getTile(getTilemapLocation(position));
    }

    //####################################################################################################
    /* Given a 2D integer tilemap position, this returns the Tile
     * Gameobject that is located at that position.  This returns the 
     * GameObject itself, which has the Tile component connected.
     * If there is no Tile located at that position, null is returned.
     */
    public GameObject getTileGameObject(Vector2Int tileLocation) {
        Vector2Int tileInChunkLocation = new Vector2Int(tileChunkMod(tileLocation.x), tileChunkMod(tileLocation.y));
        Vector2Int chunkLocation = (tileLocation - tileInChunkLocation) / TileChunk.CHUNK_SIZE;
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        if (tileChunk == null) {
            return null;
        }

        return tileChunk.getTile(tileInChunkLocation);
    }

    public GameObject getTileGameObject(Vector2 position) {
        return getTileGameObject(getTilemapLocation(position));
    }


    //####################################################################################################
    /* Returns the neighbors to a specific location in the tilemap.
     * The user can specify if they want to include the diagonal
     * tiles in the neighbor list, as well as if they want to include
     * non-walkable tiles in the list
     */
    delegate void NeighborBuilderDelegate(Vector2Int delta);
    public List<Tile> getNeighboringTiles(Vector2Int position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        List<Tile> neighborList = new List<Tile>();

        NeighborBuilderDelegate buildNeighborList = delegate(Vector2Int delta) {
            Tile tile = getTile(position + delta);
            if (tile != null) {
                if (tile.isWalkable || includeNonWalkable) {
                    neighborList.Add(tile);
                }
            }
        };

        buildNeighborList(Vector2Int.right);
        buildNeighborList(Vector2Int.left);
        buildNeighborList(Vector2Int.up);
        buildNeighborList(Vector2Int.down);
        if (includeDiagonal) {
            buildNeighborList(position + Vector2Int.right + Vector2Int.up);
            buildNeighborList(position + Vector2Int.right + Vector2Int.down);
            buildNeighborList(position + Vector2Int.left + Vector2Int.up);
            buildNeighborList(position + Vector2Int.left + Vector2Int.down);
        }
        return neighborList;
    }

    public List<Tile> getNeighboringTiles(Vector2 position, bool includeDiagonal = true) {
        return getNeighboringTiles(getTilemapLocation(position), includeDiagonal);
    }

    //####################################################################################################
    /* USUALLY ONLY FOR USE BY THE EDITOR
     * 
     * Given a Tile GameObject, place it at the given integer Tile location.  If
     * there is already a Tile GameObject at that location, it will be destroyed
     * and replaced.
     * 
     * Internally, this method handles the expansion and memory manegement needed
     * to create and maintain the arrays needed for the tilemap.
     */
#if UNITY_EDITOR
    public void setTileGameObject(Vector2Int tileLocation, GameObject newTileObject) {
        //expand the tilemap if we need to
        expandTilemapToIncludeLocation(tileLocation);
        //Get the location within the chunk where the tile is located
        Vector2Int tileInChunkLocation = new Vector2Int(tileChunkMod(tileLocation.x), tileChunkMod(tileLocation.y));
        //Get the location of the chunk where the tile is located
        Vector2Int chunkLocation = (tileLocation - tileInChunkLocation - _chunkOriginOffset * TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        //Get the specific chunk that contains the tile
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y];

        //If the chunk doesn't exist, we need to create it
        if (tileChunk == null) {
            GameObject newTileChunkGameObject = new GameObject("TileChunk(" + chunkLocation.x + "," + chunkLocation.y + ")");
            Undo.RegisterCreatedObjectUndo(newTileChunkGameObject, "Created new tile chunk game object");
            Undo.SetTransformParent(newTileChunkGameObject.transform, transform, "Moved new object to be a child of Tilemap");
            tileChunk = ScriptableObject.CreateInstance<TileChunk>();
            Undo.RegisterCreatedObjectUndo(tileChunk, "Created new tile chunk data structure");
            tileChunk.init(newTileChunkGameObject);
            Undo.RecordObject(_tilemapChunks, "Added new chunk to tilemap chunks");
            _tilemapChunks[chunkLocation.x, chunkLocation.y] = tileChunk;
        }

        GameObject currentTile = tileChunk.getTile(tileInChunkLocation);
        //If the tile already exists, we need to destroy it first
        if (currentTile != null) {
            Undo.DestroyObjectImmediate(currentTile);
        }

        newTileObject.name += "(" + tileLocation.x + "," + tileLocation.y + ")";
        Undo.SetTransformParent(newTileObject.transform, tileChunk.gameObject.transform, "Connected tile to chunk object");
        newTileObject.transform.position = new Vector3(tileLocation.x, tileLocation.y, 0) * TILE_SIZE;
        Undo.RecordObject(tileChunk, "Added new tile to a Tile Chunk");
        tileChunk.setTile(tileInChunkLocation, newTileObject);
    }
#endif

    /* The modulus of a number based around the CHUNK_SIZE.  This version
     * of modulus is not affected by negative numbers, so it is consistant
     * accross the entire number line.
     */
    private int tileChunkMod(int x) {
        return (x % TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) % TileChunk.CHUNK_SIZE;
    }

    /* Given an integer Tile location, expand the internal data structure 
     * so that the given location is contained within it.  The internal
     * data structure only ever grows as large as it needs to.
     */
    private void expandTilemapToIncludeLocation(Vector2Int tileLocation) {
        Vector2Int tilemapArrayIndex = tileLocation - _chunkOriginOffset * 8;
        //Calculate the increase in each direction that the tilemap will need to expand
        //This is the increase in the chunk tilemap, so every increase unity represents an increase in 
        // 1 chunk size int that direction
        int increaseLeftX = Mathf.Max(0, -tilemapArrayIndex.x + TileChunk.CHUNK_SIZE - 1) / TileChunk.CHUNK_SIZE;
        int increaseRightX = Mathf.Max(0, tilemapArrayIndex.x - _tilemapChunks.width * TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        int increaseUpY = Mathf.Max(0, -tilemapArrayIndex.y + TileChunk.CHUNK_SIZE - 1) / TileChunk.CHUNK_SIZE;
        int increaseDownY = Mathf.Max(0, tilemapArrayIndex.y - _tilemapChunks.height * TileChunk.CHUNK_SIZE + TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        int increaseX = Mathf.Max(increaseLeftX, increaseRightX);
        int increaseY = Mathf.Max(increaseUpY, increaseDownY);

        //If we don't need to increase at all (if the tile falls within current bounds)
        // there is no need to continue to expand
        if (increaseX == 0 && increaseY == 0) {
            return;
        }

        //Create a new array which is the propper increased size
        Array2DTC copiedArray = _tilemapChunks;
        Undo.RecordObject(this, "Updated tile chunk with a new array");
        _tilemapChunks = ScriptableObject.CreateInstance<Array2DTC>();
        Undo.RegisterCreatedObjectUndo(_tilemapChunks, "created a new tilemap object");
        _tilemapChunks.init(copiedArray.width + increaseX, copiedArray.height + increaseY);

        //Copy the old array into the new one
        for (int x = 0; x < copiedArray.width; x++) {
            for (int y = 0; y < copiedArray.height; y++) {
                _tilemapChunks[x + increaseLeftX, y + increaseUpY] = copiedArray[x, y];
            }
        }

        Undo.DestroyObjectImmediate(copiedArray);

        //The offset is at the top left corner, and so only needs to be modified
        // if we have increased in that direction
        _chunkOriginOffset = _chunkOriginOffset - new Vector2Int(increaseLeftX, increaseUpY);
    }

    public void OnDrawGizmos() {
        if (_tilemapChunks == null) {
            clearTilemap();
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
