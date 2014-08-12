using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {
    [MinValue (0)]
    public float waitTime = 1.0f;

    public float getWaitTime() {
        return waitTime;
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
