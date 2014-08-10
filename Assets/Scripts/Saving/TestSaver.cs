using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestSaver : MonoBehaviour, ISaveable {
    public int testInt = 0;

    public void onSave(SavedComponent data) {
        data.put(testInt);
    }

    public void onLoad(SavedComponent data) {
        testInt = (int)data.get();
    }

    public void Update() {
        
    }
}
