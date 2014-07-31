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
}
