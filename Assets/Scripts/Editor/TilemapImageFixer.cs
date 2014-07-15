using UnityEngine;
using UnityEditor;
using System.Collections;

[InitializeOnLoad]
public class TilemapImageFixer : MonoBehaviour {
    private static string currentScene;

    static TilemapImageFixer() {
        currentScene = EditorApplication.currentScene;
        EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
        if (EditorApplication.timeSinceStartup < 1.0f) {
            Tilemap.recalculateTileImages();
        }
    }

    private static void hierarchyWindowChanged() {
        if (currentScene != EditorApplication.currentScene) {
            currentScene = EditorApplication.currentScene;
            Tilemap.recalculateTileImages();
        }
    }
}
