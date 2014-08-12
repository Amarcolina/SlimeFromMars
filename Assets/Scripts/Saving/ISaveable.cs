using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ISaveable {
    void onSave(SavedComponent data);
    void onLoad(SavedComponent data);
}