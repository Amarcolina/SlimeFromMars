using UnityEngine;
using System.Collections;

public class TestDamageable : MonoBehaviour, IDamageable {
    public void damage(float damage) { }
    public float getHealth() { return 1.0f; }
}
