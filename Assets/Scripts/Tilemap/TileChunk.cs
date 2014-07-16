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

    public void OnDestroy() {
        DestroyImmediate(_tiles);
    }

    public GameObject getTile(Vector2Int location) {
        return _tiles[location.x, location.y];
    }

    public void setTile(Vector2Int location, GameObject tile) {
        _tiles[location.x, location.y] = tile;
    }
}
