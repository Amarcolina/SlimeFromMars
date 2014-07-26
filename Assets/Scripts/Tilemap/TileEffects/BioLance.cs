using UnityEngine;
using System.Collections;

public class BioLance : MonoBehaviour {
    Path lancePath;
    int lanceDamage;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    public void setLanceDamage(int damage) {
        lanceDamage = damage;
    }

    public void setLancePath(Path path) {
        lancePath = path;
    }
}
