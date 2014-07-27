using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileChunk : ScriptableObject {
    public const int CHUNK_SIZE = 8;
    [SerializeField]
    [HideInInspector]
    private Array2DGO _tiles = null;
    [SerializeField]
    [HideInInspector]
    private GameObject _chunkGameObject = null;

    private Array2DTile _tileComponentCache = null;

    public GameObject gameObject {
        get {
            return _chunkGameObject;
        }
    }

    /* This is the stand-in for a constructor, since ScriptableObjects
     * cannot be instantiated using constructors.  Calling this method
     * initializes the class properly.  The passed in game object is
     * a game object used by the tilemap to structure and organize
     * tiles.  Any tile within this chunk will end up being a child of
     * the given game object
     */
    public void init(GameObject chunkGameObject) {
        _tiles = ScriptableObject.CreateInstance<Array2DGO>();
        _tiles.init(CHUNK_SIZE, CHUNK_SIZE);
        _chunkGameObject = chunkGameObject;
    }

    public void OnDestroy() {
        DestroyImmediate(_tiles);
    }

    /* Used to access a tile from this chunk.  The input is the 
     * LOCAL position inside of this chunk.  Each index must
     * range from 0 to CHUNK_SIZE - 1.  
     */
    public GameObject getTile(Vector2Int location) {
        return _tiles[location.x, location.y];
    }

    public Tile getTileComponent(Vector2Int location) {
        if (_tileComponentCache == null) {
            _tileComponentCache = ScriptableObject.CreateInstance<Array2DTile>();
            _tileComponentCache.init(CHUNK_SIZE, CHUNK_SIZE);
            for (int i = 0; i < CHUNK_SIZE; i++) {
                for (int j = 0; j < CHUNK_SIZE; j++) {
                    GameObject tileObj = _tiles[i, j];
                    if (tileObj != null) {
                        _tileComponentCache[i, j] = tileObj.GetComponent<Tile>();
                    }
                }
            }
        }

        return _tileComponentCache[location.x, location.y];
    }

    /* Used to set a tile for this chunk.  The input is the 
     * LOCAL position inside of this chunk.  Each index must
     * range from 0 to CHUNK_SIZE - 1.  
     */
    public void setTile(Vector2Int location, GameObject tile) {
        _tiles[location.x, location.y] = tile;
    }
}
