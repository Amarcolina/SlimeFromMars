using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestSaver : MonoBehaviour, ISaveable {
    public int testInt = 0;

    public void onSave(Queue<object> data) {
        data.Enqueue(testInt);
    }

    public void onLoad(Queue<object> data) {
        testInt = (int)data.Dequeue();
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            SaveManager.InstantiateSaved("SaveTestPrefab", Vector3.zero, Quaternion.identity);
        }
    }
}
