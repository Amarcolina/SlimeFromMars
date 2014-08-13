using UnityEngine;
using System.Collections;

public class Vent : MonoBehaviour, IDamageable {
    private Tile _tile;

    public void Awake() {
        _tile = Tilemap.getInstance().getTile(transform.position);
        _tile.isSpikeable = true;
        _tile.isSlimeable = false;
        _tile.isTransparent = true;
    }

    public void OnDestroy() {
        _tile.isSlimeable = true;
    }

    public void damage(float damage) {
        Destroy(gameObject);
    }

    public float getHealth() {
        return 1.0f;
    }
}
