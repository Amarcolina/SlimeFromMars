using UnityEngine;
using System.Collections;

public class RadiationLeech : MonoBehaviour {
    public const float STARTLE_RADIUS = 10.0f;

    private float _damagePeriod = 2.0f;
    private float _damagePerPeriod = 0.1f;
    private IDamageable _damageable = null;

    private float _timeUntilNextDamage = 0.0f;
    private GameObject _explosionEffectPrefab;

    public void Start() {
        _damageable = GetComponent(typeof(IDamageable)) as IDamageable;
        _explosionEffectPrefab = Resources.Load<GameObject>("Particles/RadiationDefense");
    }

    public void Update() {
        _timeUntilNextDamage -= Time.deltaTime;
        if (_timeUntilNextDamage <= 0.0f) {
            _timeUntilNextDamage += _damagePeriod;
            _damageable.damage(_damagePerPeriod);
            if (_damageable.getHealth() <= 0.0f) {
                explode();
            }
        }
    }

    private void explode() {
        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();
        foreach (BaseEnemy enemy in enemies) {
            Vector3 dist = transform.position - enemy.transform.position;
            if (dist.sqrMagnitude <= STARTLE_RADIUS * STARTLE_RADIUS) {
                enemy.tryEnterState(EnemyState.STARTLED);
            }
        }

        GameObject explosionEffect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity) as GameObject;
        Destroy(explosionEffect, 2.0f);

        Destroy(gameObject);
    }

    public void setDamage(float damage) {
        _damagePerPeriod = damage;
    }

    public void setDamagePeriod(float period) {
        _damagePeriod = period;
    }
}
