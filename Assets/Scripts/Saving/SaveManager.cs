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

    /* This is called when the level is loaded, and causes the game to be modified
     * to suite the state of the current save.  
     */
    public void OnLevelWasLoaded() {
        if (_isLoading) {
            SaveMarker[] markers = FindObjectsOfType<SaveMarker>();
            foreach (SaveMarker marker in markers) {
                if (_currentSavedGame.destroyedObjects.Contains(marker.serialID)) {
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
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F10)) {
            saveGame();
        }

        if (Input.GetKeyDown(KeyCode.F11)) {
            loadGame();
        }
    }

    /* Use this if you want to Instantiate a prefab that you want to persist when
     * a game is saved and reloaded.  Using this method stores the relevant information
     * to re-create the prefab in the correct way when the game is re-loaded
     */
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

    /* This method is called by a SaveMarker whenever it is destroyed.  This keeps track
     * of any methods that have been destroyed so that when a game is re-loaded, 
     * any objects that should be destroyed can do so.
     */
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

    /* Calling this saves the current game
     */
    public void saveGame() {
        SaveMarker[] existingMarkers = FindObjectsOfType<SaveMarker>();
        foreach (SaveMarker marker in existingMarkers) {
            SavedGameObjectData savedData = marker.saveData();

            //If the saved data is null, there has been no change so we don't need to save it
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

    /* Calling this loads the game.  This first causes a level reload, after which
     * the game is modified to be equal to the save state
     */
    public void loadGame() {
        _isLoading = true;
        Application.LoadLevel(Application.loadedLevel);
    }
}
