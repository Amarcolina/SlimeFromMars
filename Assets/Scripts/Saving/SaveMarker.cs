﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SavedComponent{
    public Queue<object> savedData = new Queue<object>();
}

public class SavedGameObjectData {
    public Dictionary<System.Type, SavedComponent> componentDictionary = new Dictionary<System.Type,SavedComponent>();
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
    public string prefabPath = null;
}

public class SaveMarker : MonoBehaviour {
    [HideInInspector]
    public int serialID = -1;
    [HideInInspector]
    public string prefabPath = null;

    public void Start() {
        if (serialID == -1) {
            Debug.LogError("Object " + gameObject + " was created with an uninitialized SaveMarker serialID");
        }
    }

    public void OnDestroy() {
        SaveManager manager = FindObjectOfType<SaveManager>();
        if (manager != null) {
            manager.recordObjectDestruction(serialID);
        }
    }

    public SavedGameObjectData saveData () {
        SavedGameObjectData gameObjectData = new SavedGameObjectData();
        gameObjectData.position = transform.position;
        gameObjectData.rotation = transform.rotation;
        gameObjectData.localScale = transform.localScale;
        gameObjectData.prefabPath = prefabPath;

        Component[] saveableComponents = GetComponents(typeof(ISaveable));

        foreach (ISaveable saveable in saveableComponents) {
            SavedComponent componentData = new SavedComponent();
            componentData.savedData = new Queue<object>();
            saveable.onSave(componentData.savedData);

            if(gameObjectData.componentDictionary.ContainsKey(saveable.GetType())){
                throw new System.Exception("Cannot save component " + (saveable as Component) + " as there are two of that type on this Game Object " + gameObject);
            }
            gameObjectData.componentDictionary[saveable.GetType()] = componentData;
        }

        return gameObjectData;
	}

    public void loadData(SavedGameObjectData gameObjectData) {
        Component[] saveableComponents = GetComponents(typeof(ISaveable));
        Dictionary<System.Type, SavedComponent> copyDict = new Dictionary<System.Type, SavedComponent>(gameObjectData.componentDictionary);

        foreach (ISaveable saveable in saveableComponents) {
            System.Type componentType = saveable.GetType();

            SavedComponent savedComponent;
            if (copyDict.TryGetValue(componentType, out savedComponent)) {
                saveable.onLoad(savedComponent.savedData);
                copyDict.Remove(componentType);
            }
        }

        transform.position = gameObjectData.position;
        transform.rotation = gameObjectData.rotation;
        transform.localScale = gameObjectData.localScale;

        foreach (var extraDataPair in copyDict) {
            SavedComponent extraData = extraDataPair.Value;

            ISaveable newComponent = gameObject.AddComponent(extraDataPair.Key) as ISaveable;
            if (newComponent == null) {
                throw new System.Exception("Could not create component for type " + extraDataPair.Key);
            }
            newComponent.onLoad(extraData.savedData);
        }
    }
}