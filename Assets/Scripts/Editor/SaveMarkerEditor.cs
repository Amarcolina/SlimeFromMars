using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SaveMarker))]
public class SaveMarkerEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        SaveMarker marker = target as SaveMarker;

        string typeString = "Scene";
        if(marker.serialID == -1){
            typeString = "Uninitialized";
        }
        if(marker.serialID < -1){
            typeString = "Instance";
        }
        EditorGUILayout.LabelField("Type", typeString);

        string serialIDLabel = "" + Mathf.Abs(marker.serialID);
        if (marker.serialID == -1) {
            serialIDLabel = "";
        }

        EditorGUILayout.LabelField("Serial ID", serialIDLabel);

        if (marker.prefabPath != null && marker.prefabPath.Length > 0) {
            EditorGUILayout.LabelField("Prefab Path", marker.prefabPath);
        }
    }
}
