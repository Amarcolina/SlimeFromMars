using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ISaveable {
    void onSave(Queue<object> saveQueue);
    void onLoad(Queue<object> getQueue);
}
