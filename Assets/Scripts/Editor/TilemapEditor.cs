using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor {
    public const int PREVIEW_BORDER = 5;
    public const int PREVIEW_SIZE = 64;
    static GameObject currentTilePrefab;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        GUILayout.Label("Tilemap Editor");
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(lastRect, "");

        GameObject newTile = (GameObject)EditorGUILayout.ObjectField("Current Tile", currentTilePrefab, typeof(GameObject), false);
        if (newTile && newTile.GetComponent<Tile>()) {
            currentTilePrefab = newTile;
        }

        if (currentTilePrefab) {
            SerializedObject obj = new SerializedObject(currentTilePrefab.GetComponent<Tile>());
            SerializedProperty prop = obj.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(true)) {
                EditorGUILayout.PropertyField(prop);
            }

            if (obj.ApplyModifiedProperties()) {
                currentTilePrefab.GetComponent<Tile>().updateTileWithSettings();
            }

            Rect borderRect = GUILayoutUtility.GetRect(PREVIEW_SIZE + PREVIEW_BORDER * 2, PREVIEW_SIZE + PREVIEW_BORDER * 2);
            float diffWidth = borderRect.width - (PREVIEW_SIZE + PREVIEW_BORDER * 2);
            borderRect.x += diffWidth / 2.0f;
            borderRect.width = borderRect.width - diffWidth;

            Rect previewRect = borderRect;
            previewRect.x += PREVIEW_BORDER;
            previewRect.y += PREVIEW_BORDER;
            previewRect.width -= PREVIEW_BORDER * 2;
            previewRect.height -= PREVIEW_BORDER * 2;

            GUI.Box(borderRect, new GUIContent());
            Tile currentTile = currentTilePrefab.GetComponent<Tile>();
            drawSprite(previewRect, currentTile.groundSprite);
            drawSprite(previewRect, currentTile.groundEffectSprite);
            drawSprite(previewRect, currentTile.objectSprite);
            drawSprite(previewRect, currentTile.overlaySprite);

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (GUILayout.Button("Create copy of tile")) {
                string newPrefabPath = "Assets/Resources/TilePrefabs/" + currentTilePrefab.name + " Copy.prefab";
                string newPrefabPath2 = "TilePrefabs/" + currentTilePrefab.name + " Copy";
                PrefabUtility.CreatePrefab(newPrefabPath, currentTilePrefab);
                currentTilePrefab = Resources.Load<GameObject>(newPrefabPath2);
                Debug.Log(currentTilePrefab);
            }
        }

        if (GUILayout.Button("Recalculate tilemap textures")) {
            TilemapImageFixer.updateAllTileImages();
        }

        GUI.color = new Color(1.0f, 0.2f, 0.2f);
        if (GUILayout.Button("Reset Tilemap")) {
            if (EditorUtility.DisplayDialog("Erase Tilemap", "Are you sure you want to erase the entire tilemap?", "Erase!", "Keep it")) {
                Tilemap tilemap = target as Tilemap;
                tilemap.clear();
            }
        }
    }

    private void drawSprite(Rect rect, Sprite sprite) {
        if (sprite) {
            GUI.DrawTexture(rect, sprite.texture, ScaleMode.StretchToFill);
        }
    }

    public void OnSceneGUI() {
        int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

        switch (Event.current.type) {
            case EventType.mouseMove:
                mouseMove();
                break;
            case EventType.mouseDown:
                mouseDown();
                break;
            case EventType.MouseDrag:
                mouseDrag();
                break;
            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            default:
                break;
        }
    }

    private void mouseDown() {
        if (Event.current.button == 0 && currentTilePrefab) {
            handleMouseDraw(Event.current.mousePosition, Event.current.mousePosition);
        }
    }

    private void mouseMove() {

    }

    private void mouseDrag() {
        if (Event.current.button == 0 && currentTilePrefab) {
            handleMouseDraw(Event.current.mousePosition - Event.current.delta, Event.current.mousePosition);
        }
    }

    private void handleMouseDraw(Vector2 start, Vector2 end) {
        Undo.RecordObject(target, "Tilemap modified");

        Vector2 intersectionStart;
        if (getTilemapIntersection(start, out intersectionStart)) {
            Vector2 intersectionEnd;
            if (getTilemapIntersection(end, out intersectionEnd)) {
                Vector2Int startPosition = Tilemap.getTilemapLocation(intersectionStart);
                Vector2Int endPosition = Tilemap.getTilemapLocation(intersectionEnd);
                Vector2 startPositionV = new Vector2(startPosition.x, startPosition.y);
                Vector2 endPositionV = new Vector2(endPosition.x, endPosition.y);
                float distance = Vector2.Distance(startPositionV, endPositionV);

                if (distance > 0.5f) {
                    int prevX = 0, prevY = 0;
                    bool set = false;
                    for (float d = 0; d <= distance; d += 0.5f) {
                        Vector2 lerpedV = Vector2.Lerp(startPositionV, endPositionV, d / distance);
                        int x = (int)lerpedV.x;
                        int y = (int)lerpedV.y;
                        if (!set || prevX != x || prevY != y) {
                            drawToTilemap(new Vector2Int(x, y));
                        }
                        prevX = x;
                        prevY = y;
                        set = true;
                    }
                } else {
                    drawToTilemap(endPosition);
                }

                Event.current.Use();
            }
        }
    }

    private GameObject newTileObject() {
        GameObject newTileObject = (GameObject)PrefabUtility.InstantiatePrefab(currentTilePrefab);
        return newTileObject;
    }

    

    private void drawToTilemap(Vector2Int position){
        Tilemap tilemap = target as Tilemap;
        tilemap.setTileGameObject(position, newTileObject());
    }

    private bool getTilemapIntersection(Vector2 position, out Vector2 intersection) {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(position);
        Plane tilemapPlane = new Plane(Vector3.back, Vector3.zero);
        float dist = 0;
        if (tilemapPlane.Raycast(mouseRay, out dist)) {
            intersection = mouseRay.origin + mouseRay.direction * dist;
            return true;
        }
        intersection = Vector2.zero;
        return false;
    }
}
