using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Electrified : MonoBehaviour {
    public const float DURATION = 0.5f;

    private float _damage;
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
        _electricityEffect = Instantiate(_electricityParticleEffectPrefab) as GameObject;
        _electricityEffect.transform.position = transform.position + Vector3.back;

        Destroy(_electricityEffect, 2.0f);
        Destroy(this, DURATION);
	}

    public void setDamage(float damage) {
        _damage = damage;
    }

    /* This update will run for DURATION amount of time.  It will damage any 
     * tile enties that exist on the tile that have an IDamageable component
     * connected to their game object.  It will damage the objects a little bit
     * each frame
     */
    public void Update() {
        if (_damage != 0.0f) {
            _tile.damageTileEntities(_damage * Time.deltaTime / DURATION, false);
        }
    }
}
