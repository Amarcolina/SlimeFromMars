using UnityEngine;
using System.Collections;

public class ScientistHidingSpot : MonoBehaviour {
    public Transform enterLocation = null;

    private bool _claimed = false;

    public bool tryToClaim() {
        if (_claimed) {
            return false;
        }
        _claimed = true;
        return true;
    }

    public bool isClaimed() {
        return _claimed;
    }

    public void release() {
        _claimed = false;
    }

    public void OnDrawGizmos() {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
        if (enterLocation != null) {
            Gizmos.color = new Color(1.0f, 0.5f, 0.0f);
            Gizmos.DrawLine(transform.position, enterLocation.position);
        }
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}
