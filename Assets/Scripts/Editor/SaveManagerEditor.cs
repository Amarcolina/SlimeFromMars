using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor {
    private static Texture2D _saveIcon;
    private static Texture2D _warningIcon;
    private static Texture2D _dataIcon;

    static SaveManagerEditor() {
        _saveIcon = Resources.Load<Texture2D>("Sprites/UISprites/saveIcon");
        _warningIcon = Resources.Load<Texture2D>("Sprites/UISprites/warningIcon");
        _dataIcon = Resources.Load<Texture2D>("Sprites/UISprites/dataIcon");
        EditorApplication.hierarchyWindowItemOnGUI += hierarchyItemDrawer;
    }

    static void hierarchyItemDrawer(int instanceID, Rect drawRect){
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj != null) {
            SaveMarker marker = obj.GetComponent<SaveMarker>();
            if (marker != null) {
                Rect iconRect = drawRect;
                iconRect.x += iconRect.width - iconRect.height - 10;
                iconRect.width = iconRect.height;

                if (marker.serialID == -1) {
                    GUI.DrawTexture(iconRect, _warningIcon);
                } else {
                    GUI.DrawTexture(iconRect, _saveIcon);
                }
            }
            if (obj.GetComponent(typeof(ISaveable)) != null) {
                Rect iconRect = drawRect;
                iconRect.x += iconRect.width - iconRect.height - 30;
                iconRect.width = iconRect.height;

                GUI.DrawTexture(iconRect, _dataIcon);
            }
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
