using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slime : MonoBehaviour {
    private Tile _myTile;
    private int _slimeNeighbors = 0;

    private Tilemap _tilemap;

    public void Awake() {
        _tilemap = Tilemap.getInstance();
        _myTile = GetComponent<Tile>();
    }

    private void updateNeighborCount() {
        List<Tile> neighbors = _tilemap.getNeighboringTiles(transform.position);
        _slimeNeighbors = 0;
        foreach (Tile tile in neighbors) {
            if (tile.GetComponent<Slime>() != null) {
                _slimeNeighbors++;
            }
        }
    }
}
