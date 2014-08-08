using UnityEngine;
using System.Collections;

public class RadiationLeech : MonoBehaviour {
    private float _damagePeriod = 1.0f;
    private float _damagePerPeriod = 0.1f;
    private IDamageable _damageable = null;

    private float _timeUntilNextDamage = 0.0f;

    public void Start() {
        _damageable = GetComponent(typeof(IDamageable)) as IDamageable;
    }

    public void Update() {
        _timeUntilNextDamage -= Time.deltaTime;
        if (_timeUntilNextDamage <= 0.0f) {
            _timeUntilNextDamage += _damagePeriod;
            _damageable.damage(_damagePerPeriod);
        }
    }

    public void setDamage(float damage) {
        _damagePerPeriod = damage;
    }

    public void setDamagePeriod(float period) {
        _damagePeriod = period;
    }
}
