using UnityEngine;
using System.Collections;

public class Irradiated : MonoBehaviour {
    private float STUN_DURATION = 1.0f;
    private Tile _tile;

    private static GameObject _stunEffectPrefab = null;
    private GameObject _stunEffect = null;

    void Awake() {
        if (_stunEffectPrefab == null) {
            _stunEffectPrefab = Resources.Load<GameObject>("Particles/RadiationAura");
        }

        _stunEffect = Instantiate(_stunEffectPrefab) as GameObject;
        _stunEffect.transform.parent = transform;
        _stunEffect.transform.position = transform.position + Vector3.back;

        _tile = GetComponent<Tile>();
        _tile.damageTileEntities(0, true);
    }

    /* This update will run for DURATION amount of time.  It will damage any 
     * tile enties that exist on the tile that have an IDamageable component
     * connected to their game object.  It will damage the objects a little bit
     * each frame
     */
    public void Update() {
        _tile.stunTileEntities(STUN_DURATION);
    }
}
