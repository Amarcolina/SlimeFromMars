using UnityEngine;
using System.Collections;

public class TileEntity : MonoBehaviour {
    private Tilemap _tilemap;
    private SpriteRenderer _spriteRenderer;

    struct TilePositionStruct {
        public Vector2Int tilePos;
        public Tile tile;
    }

    private TilePositionStruct[] _currentList = new TilePositionStruct[9];
    private TilePositionStruct[] _newList = new TilePositionStruct[9];


    public void Awake() {
        _tilemap = Tilemap.getInstance();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        generateNewList();
        updateCurrentWithNew();
        addToTileEntities();

        //Debug.Log(_spriteRenderer.sprite.bounds.center + transform.position);

        if (gameObject.isStatic) {
            enabled = false;
        }
    }

    private void generateNewList() {
        for (int x = 0; x < 3; x++) {
            float xPercent = (x - 1.0f) * 0.95f;
            for (int y = 0; y < 3; y++) {
                int index = y * 3 + x;
                float yPercent = (y - 1.0f) * 0.95f;
                Vector3 extent = _spriteRenderer.sprite.bounds.extents;
                extent.x *= xPercent * transform.localScale.x;
                extent.y *= yPercent * transform.localScale.y;
                _newList[index].tilePos = _spriteRenderer.sprite.bounds.center + extent + transform.position;
                _newList[index].tile = _tilemap.getTile(_newList[index].tilePos);
            }
        }
    }

    public void OnDestroy() {
        removeFromTileEntities();
    }

    private void addToTileEntities() {
        for (int i = 0; i < _currentList.Length; i++) {
            if (_currentList[i].tile != null) {
                _currentList[i].tile.addTileEntity(this);
            }
        }
    }

    private void removeFromTileEntities() {
        for (int i = 0; i < _currentList.Length; i++) {
            if (_currentList[i].tile != null) {
                _currentList[i].tile.removeTileEntity(this);
            }
        }
    }

    private void updateCurrentWithNew() {
        for (int i = 0; i < _currentList.Length; i++) {
            _currentList[i] = _newList[i];
        }
    }

    public void forceUpdate() {
        generateNewList();

        bool didChange = false;
        for (int i = 0; i < _currentList.Length; i++) {
            if (_currentList[i].tilePos != _newList[i].tilePos) {
                didChange = true;
                break;
            }
        }

        if (didChange) {
            removeFromTileEntities();
            updateCurrentWithNew();
            addToTileEntities();
        }
    }

    public void Update() {
        forceUpdate();
    }
}
