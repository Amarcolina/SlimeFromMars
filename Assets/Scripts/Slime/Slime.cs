using UnityEngine;
using System.Collections;

public class Slime : MonoBehaviour {
    private Tile _myTile;
    private int _emptyNeighbors = 0;

    public void Awake() {
        _myTile = GetComponent<Tile>();
    }

    private void updateNeighborCount() {
        Tilemap tilemap = null;
        
    }
}
