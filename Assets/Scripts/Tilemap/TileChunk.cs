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
