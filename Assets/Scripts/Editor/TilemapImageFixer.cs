using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[InitializeOnLoad]
public class TilemapImageFixer : MonoBehaviour {
    private static string currentScene;

    static TilemapImageFixer() {
        currentScene = EditorApplication.currentScene;
        EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
        if (EditorApplication.timeSinceStartup < 1.0f) {
            updateAllTileImages();
        }
    }

    private static void hierarchyWindowChanged() {
        if (currentScene != EditorApplication.currentScene) {
            currentScene = EditorApplication.currentScene;
            updateAllTileImages();
        }
    }

    public static void updateAllTileImages() {
        TextureCombiner.clearCachedSprites();
        string[] tilePaths = Directory.GetFiles("Assets/Resources/TilePrefabs/", "*.prefab");
        foreach (string totalPath in tilePaths) {
            string assetPath = totalPath.Substring("Assets/Resources/".Length);
            assetPath = assetPath.Substring(0, assetPath.Length - ".prefab".Length);

            GameObject tilePrefab = (GameObject)Resources.Load(assetPath);
            tilePrefab.GetComponent<Tile>().updateTileWithSettings();
        }
    }
}
