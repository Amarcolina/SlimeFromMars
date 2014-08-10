using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor {

    static SaveManagerEditor() {
        EditorApplication.hierarchyWindowItemOnGUI += hierarchyItemDrawer;
    }

    static void hierarchyItemDrawer(int instanceID, Rect drawRect){
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj != null && obj.name[0] == 'A') {
            GUI.Box(drawRect, "");
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Separator();
        if (GUILayout.Button("Assign Serial IDs")) {
            assignSerialIDs();
        }
        EditorGUILayout.Separator();
        EditorGUILayout.EndHorizontal();

        if (SaveManager._currentSavedGame != null) {
            EditorGUILayout.LabelField("Modified Objects", "" + SaveManager._currentSavedGame.modifiedGameObjects.Count);
            EditorGUILayout.LabelField("New Objects", "" + SaveManager._currentSavedGame.newGameObjects.Count);
            EditorGUILayout.LabelField("Deleted Objects", "" + SaveManager._currentSavedGame.destroyedObjects.Count);
        }
    }

    public void assignSerialIDs() {
        SaveMarker[] existingMarkers = FindObjectsOfType<SaveMarker>();
        for (int i = 0; i < existingMarkers.Length; i++) {
            SerializedObject serializedObject = new SerializedObject(existingMarkers[i]);
            SerializedProperty property = serializedObject.FindProperty("serialID");
            property.intValue = i;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.DisplayProgressBar("Assigning Serial ID's", "just a moment...", i / (float)existingMarkers.Length);
        }
        EditorUtility.ClearProgressBar();
    }
}
