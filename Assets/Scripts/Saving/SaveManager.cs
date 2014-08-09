using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour {
    private static SavedGame _currentSavedGame = null;

    public class SavedGame{
        public Dictionary<int, SavedGameObjectData> modifiedGameObjects = new Dictionary<int, SavedGameObjectData>();
        public Dictionary<int, SavedGameObjectData> newGameObjects = new Dictionary<int, SavedGameObjectData>();
        public HashSet<int> destroyedObjects = new HashSet<int>();
    }

    public void OnEnable() {
        if (_currentSavedGame != null) {
            SaveMarker[] markers = FindObjectsOfType<SaveMarker>();
            foreach(SaveMarker marker in markers){
                if(_currentSavedGame.destroyedObjects.Contains(marker.serialID)){
                    Destroy(marker.gameObject);
                }

                SavedGameObjectData data;
                if (_currentSavedGame.modifiedGameObjects.TryGetValue(marker.serialID, out data)) {
                    marker.loadData(data);
                }
            }

            Debug.Log(1);
            foreach (var newObjectPair in _currentSavedGame.newGameObjects) {
                Debug.Log(2);
                GameObject newObject = new GameObject("RIP omg");
                SaveMarker newMarker = newObject.AddComponent<SaveMarker>();
                newMarker.serialID = newObjectPair.Key;
                newMarker.loadData(newObjectPair.Value);
            }

            _currentDestroyedObjects.Clear();
            _currentDestroyedObjects.UnionWith(_currentSavedGame.destroyedObjects);
        } else {
            _currentSavedGame = new SavedGame();
        }
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            saveGame();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            loadGame();
        }
    }

    [ContextMenu ("Assign Serial IDs")]
    public void assignSerialIDs() {
        SaveMarker[] existingMarkers = FindObjectsOfType<SaveMarker>();
        for (int i = 0; i < existingMarkers.Length; i++) {
            existingMarkers[i].serialID = i;
        }
    }

    private HashSet<int> _currentDestroyedObjects = new HashSet<int>();
    public void recordObjectDestruction(int serialID) {
        if (serialID < -1) {
            _currentSavedGame.newGameObjects.Remove(serialID);
        } else if (serialID > -1) {
            _currentDestroyedObjects.Add(serialID);
        } else {
            throw new System.Exception("Destroyed an object that had an uninitialized serialID");
        }
    }

    public void saveGame() {
        SaveMarker[] existingMarkers = FindObjectsOfType<SaveMarker>();
        foreach (SaveMarker marker in existingMarkers) {
            if (marker.serialID < -1) {
                Debug.Log("here???");
                _currentSavedGame.newGameObjects[marker.serialID] = marker.saveData();
            } else if (marker.serialID > -1){
                _currentSavedGame.modifiedGameObjects[marker.serialID] = marker.saveData();
            } else{
                throw new System.Exception("Trying to save an object with an uninitialized serialID");
            }
        }

        _currentSavedGame.destroyedObjects.UnionWith(_currentDestroyedObjects);
    }

    public void loadGame() {
        Application.LoadLevel(Application.loadedLevel);
    }
}
