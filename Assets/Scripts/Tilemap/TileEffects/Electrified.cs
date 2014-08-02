using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Electrified : MonoBehaviour {
    public const float DURATION = 1.0f;
    public const float TOTAL_DAMAGE = 0.1f;

    private float _totalTime = 0.0f;
    private Tile _tile;
    private static GameObject _electricityParticleEffectPrefab = null;
    private GameObject _electricityEffect = null;

    /* Sprites are static so they can be shared between all 
     * Electried components.  This method is called by the 
     * first Electrified component to initialize the needed
     * resources
     */
    private void initElectricPrefab() {
        _electricityParticleEffectPrefab = Resources.Load<GameObject>("Particles/ElectricGround");
    }

	void Awake () {
        if (_electricityParticleEffectPrefab == null) {
            initElectricPrefab();
        }

        _tile = GetComponent<Tile>();
        _tile.damageTileEntities(0, true);
        _electricityEffect = Instantiate(_electricityParticleEffectPrefab) as GameObject;
        _electricityEffect.transform.position = transform.position + Vector3.back;
	}

    public void OnDestroy() {
        Destroy(_electricityEffect);
    }

    /* This update will run for DURATION amount of time.  It will damage any 
     * tile enties that exist on the tile that have an IDamageable component
     * connected to their game object.  It will damage the objects a little bit
     * each frame
     */
    public void Update() {
        _tile.damageTileEntities(TOTAL_DAMAGE * Time.deltaTime / DURATION, false);

        _totalTime += Time.deltaTime;
        if (_totalTime >= DURATION) {
            Destroy(this);
        }
    }
}
