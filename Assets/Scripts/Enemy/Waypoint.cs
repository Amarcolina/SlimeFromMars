using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {
    public void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1.0f);
    }
}
