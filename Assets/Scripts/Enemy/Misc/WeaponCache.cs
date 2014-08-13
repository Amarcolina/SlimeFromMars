using UnityEngine;
using System.Collections;

public class WeaponCache : MonoBehaviour {
    public Transform pathingWaypoint;
    [MinValue (0)]
    public int bulletsInCache = 50;

    public int takeBullets(int maxBulletsToTake) {
        int takenBullets = Mathf.Min(bulletsInCache, maxBulletsToTake);
        bulletsInCache -= takenBullets;

        if (bulletsInCache == 0) {
            Destroy(gameObject);
        }

        return takenBullets;
    }
}
