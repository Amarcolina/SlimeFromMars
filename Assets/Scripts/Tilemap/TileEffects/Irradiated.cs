using UnityEngine;
using System.Collections;

public class Irradiated : MonoBehaviour {
    public const float DURATION = 1.0f;
    public const float TOTAL_DAMAGE = 0.1f;
    private bool shouldStun;
    private bool shouldDamage;
    private float _totalTime = 0.0f;
    private Tile _tile;

    private static GameObject _stunEffectPrefab = null;
    private static GameObject _damageEffectPrefab = null;
    private GameObject _stunEffect = null;
    private GameObject _damageEffect = null;

    void Awake() {
        if (_stunEffectPrefab == null) {
            _stunEffectPrefab = Resources.Load<GameObject>("Particles/RadiationAura");
            _damageEffectPrefab = Resources.Load<GameObject>("Particles/RadiationDefense");
        }

        _tile = GetComponent<Tile>();
        _tile.damageTileEntities(0, true);
    }

    public void setStunned(bool stun) {
        if (stun != shouldStun) {
            shouldStun = stun;
            if (shouldStun) {
                _stunEffect = Instantiate(_stunEffectPrefab) as GameObject;
                _stunEffect.transform.position = transform.position + Vector3.back;
            } else {
                _stunEffect.GetComponent<ParticleSystem>().enableEmission = false;
                Destroy(_stunEffect, 2.0f);
                _stunEffect = null;
            }
        }
    }

    public void setDamaged(bool damage) {
        if (damage != shouldDamage) {
            shouldDamage = damage;
            if (shouldDamage) {
                _totalTime = 0;
                _damageEffect = Instantiate(_damageEffectPrefab) as GameObject;
                _damageEffect.transform.position = transform.position + Vector3.back;
            } else {
                _damageEffect.GetComponent<ParticleSystem>().enableEmission = false;
                Destroy(_damageEffect, 2.0f);
                _damageEffect = null;
            }
        }
    }


    /* This update will run for DURATION amount of time.  It will damage any 
     * tile enties that exist on the tile that have an IDamageable component
     * connected to their game object.  It will damage the objects a little bit
     * each frame
     */
    public void Update() {
        if (shouldDamage) {
            _tile.damageTileEntities(TOTAL_DAMAGE * Time.deltaTime / DURATION, false);

            _totalTime += Time.deltaTime;
            if (_totalTime >= DURATION ) {
                setDamaged(false);
            }
        }


        if (shouldStun) {
            _tile.stunTileEntities(DURATION);
        }
    }
}
