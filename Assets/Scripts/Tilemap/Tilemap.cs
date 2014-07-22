using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;


public class Tilemap : MonoBehaviour {
    public const float TILE_SIZE = 1.0f;
    [SerializeField]
    [HideInInspector]
    private Array2D _tilemapChunks = null;
    [SerializeField]
    [HideInInspector]
    public Vector2Int _chunkOriginOffset = new Vector2Int(0, 0);

    private static Tilemap _tilemapInstance;
    public static Tilemap getInstance() {
        if (_tilemapInstance == null) {
            _tilemapInstance = FindObjectOfType<Tilemap>();
        }
        return _tilemapInstance;
    }
    
    /* Clears the tilemap of all tiles, and sets up all internal
     * variables to contain an empty tilemap.  This properly destroys
     * all internal variables and external tiles
     */
    public void clearTilemap() {
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child && child != transform) {
                DestroyImmediate(child.gameObject);
            }
        }

        if (_tilemapChunks) {
            for (int i = 0; i < _tilemapChunks.width; i++) {
                for (int j = 0; j < _tilemapChunks.height; j++) {
                    DestroyImmediate(_tilemapChunks[i, j] as TileChunk);
                }
            }
            DestroyImmediate(_tilemapChunks);
        }

        _tilemapChunks = ScriptableObject.CreateInstance<Array2D>();
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
    /* Returns whether or not a given position represents a space that can
     * be walked on.  If there is no tile at the given location, this method
     * returns false.  If there is a tile at the given location, this method
     * returns whether or not that tile is walkable
     */

    public bool isWalkable(Vector2Int position) {
        Tile tile = getTile(position);
        if (tile == null) {
            return false;
        }
        return tile.isWalkable;
    }

    public bool isWalkable(Vector2 position) {
        return isWalkable(getTilemapLocation(position));
    }

    //####################################################################################################
    /* Given a 2D integer tilemap position, this returns the Tile
     * Gameobject that is located at that position.  This returns the 
     * GameObject itself, which has the Tile component connected.
     * If there is no Tile located at that position, null is returned.
     */
    public GameObject getTileGameObject(Vector2Int tileLocation) {
        Vector2Int tileInChunkLocation = new Vector2Int(tileChunkMod(tileLocation.x), tileChunkMod(tileLocation.y));
        Vector2Int chunkLocation = (tileLocation - tileInChunkLocation - _chunkOriginOffset * TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;

        TileChunk tileChunk = null;
        if (_tilemapChunks.isInRange(chunkLocation)) {
            tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y] as TileChunk;
        }

        if (tileChunk == null) {
            return null;
        }

        return tileChunk.getTile(tileInChunkLocation);
    }

    public GameObject getTileGameObject(Vector2 position) {
        return getTileGameObject(getTilemapLocation(position));
    }

    //####################################################################################################
    /* Returns the neighboring position to a specific location in the tilemap.
     * The user can specify if they want to include the diagonal tiles in the 
     * neighbor list, as well as if they want to include non-walkable tiles in the list
     */
    public List<Vector2Int> getNeighboringPositions(Vector2Int position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        List<Vector2Int> neighborList = new List<Vector2Int>();
        FoundNeighborFunc func = delegate(Vector2Int tilePosition) {
            neighborList.Add(tilePosition);
        };
        findNeighboringLocationsInternal(position, func, includeNonWalkable, includeDiagonal);
        return neighborList;
    }

    public List<Vector2Int> getNeighboringPositions(Vector2 position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        return getNeighboringPositions(getTilemapLocation(position), includeNonWalkable, includeDiagonal);
    }

    public List<Tile> getNeighboringTiles(Vector2Int position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        List<Tile> neighborList = new List<Tile>();
        FoundNeighborFunc func = delegate(Vector2Int tilePosition) {
            neighborList.Add(getTile(tilePosition));
        };
        findNeighboringLocationsInternal(position, func, includeNonWalkable, includeDiagonal);
        return neighborList;
    }

    public List<Tile> getNeighboringTiles(Vector2 position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        return getNeighboringTiles(getTilemapLocation(position), includeNonWalkable, includeDiagonal);
    }

    private delegate void NeighborBuilderDelegate(Vector2Int delta);
    private delegate void FoundNeighborFunc(Vector2Int tilePosition);
    private void findNeighboringLocationsInternal(Vector2Int position, FoundNeighborFunc func, bool includeNonWalkable = false, bool includeDiagonal = true){
        NeighborBuilderDelegate buildNeighborList = delegate(Vector2Int delta) {
            Tile tile = getTile(position + delta);
            if (tile != null) {
                if (tile.isWalkable || includeNonWalkable) {
                    func(position + delta);
                }
            }
        };

        if (includeDiagonal) {
            if (isWalkable(position + Vector2Int.right)) {
                if (isWalkable(position + Vector2Int.up)) {
                    buildNeighborList(Vector2Int.right + Vector2Int.up);
                }
                if (isWalkable(position + Vector2Int.down)) {
                    buildNeighborList(Vector2Int.right + Vector2Int.down);
                }
            }
            if (isWalkable(position + Vector2Int.left)) {
                if (isWalkable(position + Vector2Int.up)) {
                    buildNeighborList(Vector2Int.left + Vector2Int.up);
                }
                if (isWalkable(position + Vector2Int.down)) {
                    buildNeighborList(Vector2Int.left + Vector2Int.down);
                }
            }
        }
        buildNeighborList(Vector2Int.right);
        buildNeighborList(Vector2Int.left);
        buildNeighborList(Vector2Int.up);
        buildNeighborList(Vector2Int.down);
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
    public void setTileGameObject(Vector2Int tilePosition, GameObject tileGameObject) {
        if (_tilemapChunks == null) {
            clearTilemap();
        }

        Tile tileObject = tileGameObject.GetComponent<Tile>();
        Vector2Int tileSize = tileObject.getTileSize();
        for (int i = 0; i < tileSize.x; i++) {
            for (int j = 0; j < tileSize.y; j++) {
                setSingleTileInternal(tilePosition + new Vector2Int(i, j), tileGameObject, i == 0 && j == 0);
            }
        }
    }

    private void setSingleTileInternal(Vector2Int tilePosition, GameObject tileObject, bool updateTilePosition) {
        //expand the tilemap if we need to
        expandTilemapToIncludeLocation(tilePosition);
        //Get the location within the chunk where the tile is located
        Vector2Int tileInChunkLocation = new Vector2Int(tileChunkMod(tilePosition.x), tileChunkMod(tilePosition.y));
        //Get the location of the chunk where the tile is located
        Vector2Int chunkLocation = (tilePosition - tileInChunkLocation - _chunkOriginOffset * TileChunk.CHUNK_SIZE) / TileChunk.CHUNK_SIZE;
        //Get the specific chunk that contains the tile
        TileChunk tileChunk = _tilemapChunks[chunkLocation.x, chunkLocation.y] as TileChunk;

        //If the chunk doesn't exist, we need to create it
        if (tileChunk == null) {
            GameObject newTileChunkGameObject = new GameObject("TileChunk(" + chunkLocation.x + "," + chunkLocation.y + ")");
            newTileChunkGameObject.transform.parent = transform;
            tileChunk = ScriptableObject.CreateInstance<TileChunk>();
            tileChunk.init(newTileChunkGameObject);
            _tilemapChunks[chunkLocation.x, chunkLocation.y] = tileChunk;
        }

        GameObject currentTile = tileChunk.getTile(tileInChunkLocation);
        //If the tile already exists, we need to destroy it first
        if (currentTile != null) {
            DestroyImmediate(currentTile);
        }

        Undo.RecordObject(tileChunk, "Added new tile to a Tile Chunk");
        tileChunk.setTile(tileInChunkLocation, tileObject);

        if (updateTilePosition) {
            tileObject.name += "(" + tilePosition.x + "," + tilePosition.y + ")";
            Undo.SetTransformParent(tileObject.transform, tileChunk.gameObject.transform, "Connected tile to chunk object");
            Tile tile = tileObject.GetComponent<Tile>();
            Vector2Int positionOffset = tile.getTileSize() - new Vector2Int(1, 1);
            Vector3 posOff = new Vector3(positionOffset.x, positionOffset.y, 0) / 2.0f;
            tileObject.transform.position = (new Vector3(tilePosition.x, tilePosition.y, 0) + posOff) * TILE_SIZE;   
        }
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
        Array2D newChunkArray = ScriptableObject.CreateInstance<Array2D>();
        newChunkArray.init(_tilemapChunks.width + increaseX, _tilemapChunks.height + increaseY);

        //Copy the old array into the new one
        for (int x = 0; x < _tilemapChunks.width; x++) {
            for (int y = 0; y < _tilemapChunks.height; y++) {
                newChunkArray[x + increaseLeftX, y + increaseUpY] = _tilemapChunks[x, y];
            }
        }

        //Destroy the old tilemap and assign the new one
        DestroyImmediate(_tilemapChunks);
        _tilemapChunks = newChunkArray;

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

                TileChunk chunk = _tilemapChunks[x, y] as TileChunk;
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
