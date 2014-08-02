﻿using UnityEngine;
using System.Collections;

public class TileEntity : MonoBehaviour {
    private Vector2Int _currentTilePosition;
    private Tilemap _tilemap;
    private Tile _currentTile;

    public void Awake() {
        _tilemap = Tilemap.getInstance();
        _currentTilePosition = transform.position;
        _currentTile = _tilemap.getTile(_currentTilePosition);
        _currentTile.addTileEntity(this);

        if (gameObject.isStatic) {
            enabled = false;
        }
    }

    public void OnDestroy() {
        _currentTile.removeTileEntity(this);
    }

    public void Update() {
        Vector2Int newPosition = transform.position;
        if (newPosition != _currentTilePosition) {
            if (_currentTile) {
                _currentTile.removeTileEntity(this);
            }
            _currentTilePosition = newPosition;
            _currentTile = _tilemap.getTile(_currentTilePosition);
            if (_currentTile) {
                _currentTile.addTileEntity(this);
            }
        }
    }
}
