using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour {
    public static SavedGame _currentSavedGame = null;
    private static bool _isLoading = false;

    public class SavedGame{
        public Dictionary<int, SavedGameObjectData> modifiedGameObjects = new Dictionary<int, SavedGameObjectData>();
        public Dictionary<int, SavedGameObjectData> newGameObjects = new Dictionary<int, SavedGameObjectData>();
        public HashSet<int> destroyedObjects = new HashSet<int>();
    }

    public void Awake() {
        if (_currentSavedGame == null) {
            _currentSavedGame = new SavedGame();
        }
    }

    public void OnLevelWasLoaded() {
        SaveMarker[] markers = FindObjectsOfType<SaveMarker>();
        foreach(SaveMarker marker in markers){
            if(_currentSavedGame.destroyedObjects.Contains(marker.serialID)){
                Destroy(marker.gameObject);
                continue;
            }

            SavedGameObjectData data;
            if (_currentSavedGame.modifiedGameObjects.TryGetValue(marker.serialID, out data)) {
                marker.loadData(data);
                continue;
            }
        }

        foreach (var newObjectPair in _currentSavedGame.newGameObjects) {
            GameObject newObject;
            SaveMarker newMarker;
            if (newObjectPair.Value.prefabPath != null && newObjectPair.Value.prefabPath.Length != 0) {
                newObject = InstantiateSaved(newObjectPair.Value.prefabPath, Vector3.zero, Quaternion.identity);
                newMarker = newObject.GetComponent<SaveMarker>();
            } else {
                newObject = new GameObject("Recreated Game Object");
                newMarker = newObject.AddComponent<SaveMarker>();
            }
                
            newMarker.serialID = newObjectPair.Key;
            newMarker.loadData(newObjectPair.Value);
        }

        _currentDestroyedObjects.Clear();
        _currentDestroyedObjects.UnionWith(_currentSavedGame.destroyedObjects);

        _isLoading = false;
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            saveGame();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            loadGame();
        }

        if (Input.GetKeyDown(KeyCode.I)) {
            SaveManager.InstantiateSaved("SaveTestPrefab", Vector3.zero, Quaternion.identity);
        }
    }

    public static GameObject InstantiateSaved(string prefabPath, Vector3 position, Quaternion rotation) {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject obj = Instantiate(prefab, position, rotation) as GameObject;
        SaveMarker marker = obj.GetComponent<SaveMarker>();
        if (marker == null) {
            Debug.LogWarning(obj + " was instantiated using InstantiateSaved even though it has no SaveMarker");
        } else {
            marker.prefabPath = prefabPath;
            marker.serialID = Random.Range(int.MinValue, -2);
        }
        return obj;
    }

    private HashSet<int> _currentDestroyedObjects = new HashSet<int>();
    public void recordObjectDestruction(int serialID) {
        if (_isLoading) {
            return;
        }

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
            SavedGameObjectData savedData = marker.saveData();
            if (savedData == null) {
                continue;
            }

            if (marker.serialID < -1) {
                _currentSavedGame.newGameObjects[marker.serialID] = savedData;
            } else if (marker.serialID > -1){
                _currentSavedGame.modifiedGameObjects[marker.serialID] = savedData;
            } else{
                throw new System.Exception("Trying to save an object with an uninitialized serialID");
            }
        }

        _currentSavedGame.destroyedObjects.UnionWith(_currentDestroyedObjects);
    }

    public void loadGame() {
        _isLoading = true;
        Application.LoadLevel(Application.loadedLevel);
    }
}
