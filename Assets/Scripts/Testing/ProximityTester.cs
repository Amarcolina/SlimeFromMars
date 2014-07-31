using UnityEngine;
using System.Collections;

public class ProximityTester : MonoBehaviour {
    private ProximitySearcher<Transform> _searcher;

    public void Awake() {
        Transform[] array = FindObjectsOfType<Transform>();
        Transform[] sarray = new Transform[array.Length - 1];

        int index = 0;
        foreach(Transform t in array){
            if (t != transform){
                sarray[index] = t;
                index++;
            }
        }

        _searcher = new ProximitySearcher<Transform>(sarray, 5);
    }

    public void Update() {
        _searcher.searchForClosest(transform.position, 9, 1);
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, _searcher.getCurrentClosest().position);

        Gizmos.color = Color.blue;
        for (int i = 0; i < _searcher.getNumberInLocalGroup(); i++) {
            Gizmos.DrawSphere(_searcher[i].position, 0.55f);
        }
    }
}
