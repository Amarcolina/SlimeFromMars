using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor {
    public const int PREVIEW_BORDER = 5;
    public const int PREVIEW_SIZE = 64;

    public const int SCROLL_SIZE = 32;
    public const int SCROLL_BORDER = 2;
    public const int SCROLL_HORIZONTAL_COUNT = 8;
    public const int SCROLL_VERTICAL_COUNT = 3;

    static GameObject currentTilePrefab;
    static Vector2 prefabScroll = Vector2.zero;

    public List<GameObject> _tilePrefabs = new List<GameObject>();

    public void OnEnable() {
        refreshTilePrefabList();
    }

    private void refreshTilePrefabList() {
        string[] paths = Directory.GetFiles("Assets/Resources/TilePrefabs/", "*.prefab");
        _tilePrefabs.Clear();
        foreach (string path in paths) {
            string resourcePath = path.Substring("Assets/Resources/".Length);
            resourcePath = resourcePath.Substring(0, resourcePath.Length - ".prefab".Length);
            _tilePrefabs.Add((GameObject)Resources.Load(resourcePath));
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        displayEditorHeader();
        displayTileChoiceScroller();

        if (currentTilePrefab != null) {
            displayTileFieldEditor();
            displayTilePreview();

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            displayCopyButton();
        }

        displayResetButton();
    }

    private void displayEditorHeader() {
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        GUILayout.Label("Tilemap Editor");
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(lastRect, "");

        GameObject newTile = (GameObject)EditorGUILayout.ObjectField("Current Tile", currentTilePrefab, typeof(GameObject), false);
        if (newTile && newTile.GetComponent<Tile>()) {
            currentTilePrefab = newTile;
        }
    }

    private void displayTileChoiceScroller() {
        float height = Mathf.Min(SCROLL_SIZE * SCROLL_VERTICAL_COUNT, (Mathf.Floor(_tilePrefabs.Count / (float)SCROLL_HORIZONTAL_COUNT) + 1) * SCROLL_SIZE);
        prefabScroll = GUILayout.BeginScrollView(prefabScroll, GUILayout.Height(height));
        int index = 0;
        while (index < _tilePrefabs.Count) {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < SCROLL_HORIZONTAL_COUNT; i++) {
                Rect buttonRect = GUILayoutUtility.GetRect(SCROLL_SIZE, SCROLL_SIZE);
                Rect textureRect = buttonRect;
                textureRect.x += SCROLL_BORDER;
                textureRect.y += SCROLL_BORDER;
                textureRect.width -= SCROLL_BORDER * 2;
                textureRect.height -= SCROLL_BORDER * 2;
                if (index < _tilePrefabs.Count) {
                    GameObject prefab = _tilePrefabs[index];
                    if (prefab == currentTilePrefab) {
                        GUI.backgroundColor = Color.blue;
                    }
                    if (GUI.Button(buttonRect, "")) {
                        currentTilePrefab = prefab;
                    }
                    GUI.backgroundColor = Color.white;

                    drawSprite(textureRect, prefab.GetComponent<Tile>().groundSprite);
                    drawSprite(textureRect, prefab.GetComponent<Tile>().groundEffectSprite);
                    drawSprite(textureRect, prefab.GetComponent<Tile>().objectSprite);
                    drawSprite(textureRect, prefab.GetComponent<Tile>().overlaySprite);
                }
                index++;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    private void displayTileFieldEditor() {
        SerializedObject obj = new SerializedObject(currentTilePrefab.GetComponent<Tile>());
        SerializedProperty prop = obj.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(true)) {
            EditorGUILayout.PropertyField(prop);
        }

        if (obj.ApplyModifiedProperties()) {
            updateTilePrefab();
        }
    }

    private void displayTilePreview() {
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
    }

    private void displayCopyButton() {
        if (GUILayout.Button("Create copy of tile")) {
            string newPrefabPath = "Assets/Resources/TilePrefabs/" + currentTilePrefab.name + " Copy.prefab";
            string newPrefabPath2 = "TilePrefabs/" + currentTilePrefab.name + " Copy";
            PrefabUtility.CreatePrefab(newPrefabPath, currentTilePrefab);
            currentTilePrefab = Resources.Load<GameObject>(newPrefabPath2);
            _tilePrefabs.Add(currentTilePrefab);
        }
    }

    private void displayResetButton() {
        GUI.color = new Color(1.0f, 0.2f, 0.2f);
        if (GUILayout.Button("Reset Tilemap")) {
            if (EditorUtility.DisplayDialog("Erase Tilemap", "Are you sure you want to erase the entire tilemap?", "Erase!", "Keep it")) {
                Tilemap tilemap = target as Tilemap;
                tilemap.clearTilemap();
            }
        }
        GUI.color = Color.white;
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

    private void drawSprite(Rect rect, Sprite sprite) {
        if (sprite) {
            GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit);
        }
    }

    private void updateTilePrefab() {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(currentTilePrefab);
        obj.hideFlags = HideFlags.HideAndDontSave;
        obj.GetComponent<Tile>().updateTileWithSettings();
        PrefabUtility.ReplacePrefab(obj, currentTilePrefab);
        DestroyImmediate(obj);
    }

    private void mouseDown() {
        if (Event.current.button == 0 && currentTilePrefab) {
            handleMouseDraw(Event.current.mousePosition, Event.current.mousePosition);
        }
    }

    private void mouseMove() {
        //TODO: Possible mouse cell highlight
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
