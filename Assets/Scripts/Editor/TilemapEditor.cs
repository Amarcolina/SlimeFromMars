using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor {
    public const int PREVIEW_BORDER = 9;
    public const int PREVIEW_SIZE = 64;

    public const int SCROLL_SIZE = 32;
    public const int SCROLL_BORDER = 2;
    public const int SCROLL_HORIZONTAL_COUNT = 8;
    public const int SCROLL_VERTICAL_COUNT = 3;

    static GameObject currentTilePrefab;
    static Vector2 prefabScroll = Vector2.zero;

    private List<GameObject> _tilePrefabs = new List<GameObject>();
    private float _spriteAngle = 0.0f;

    public void OnEnable() {
        refreshTilePrefabList();
    }

    /* Draws the new inspector interfatce for the tilemap.  This allows
     * the user to select the current tile that they are drawing with, as
     * well as modify tiles, create new tiles, and reset the tilemap.
     */
    public override void OnInspectorGUI() {
        displayEditorHeader();
        displayTileChoiceScroller();

        if (currentTilePrefab != null) {
            displayTileFieldEditor();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            displayTilePreview();
            displayCopyButton();
            displayTransformControls();
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
        }

        displayResetButton();
    }

    /* Draws the header for the editor.  This includes the Title bar, as
     * well as the Current Tile field which allows you to select the tile
     * from the drop down
     */
    private void displayEditorHeader() {
        GUILayout.Label("Tilemap Editor");
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(lastRect, "");

        GameObject newTile = (GameObject)EditorGUILayout.ObjectField("Current Tile", currentTilePrefab, typeof(GameObject), false);
        if (newTile && newTile.GetComponent<Tile>() && newTile != currentTilePrefab) {
            currentTilePrefab = newTile;
            _spriteAngle = 0.0f;
        }
    }

    /* Draws the scrollable field where you are able to select a
     * tile by clicking on it's preview.
     */
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
                    GUI.backgroundColor = _tilePrefabs[index].GetComponent<Tile>().minimapColor;
                    if (prefab == currentTilePrefab) {
                        GUI.backgroundColor = Color.blue;
                    }
                    if (GUI.Button(buttonRect, "")) {
                        currentTilePrefab = prefab;
                        _spriteAngle = 0.0f;
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

    /* Draws the dynamic editor which allows the user to edit the tile
     * as if it was in the inspector.  This allows access to all fields
     * of the Tile component of the tile.
     */
    private void displayTileFieldEditor() {
        SerializedObject obj = new SerializedObject(currentTilePrefab.GetComponent<Tile>());

        Tile currentTile = currentTilePrefab.GetComponent<Tile>();
        Vector2Int currentTileSize = currentTile.getTileSize();

        SerializedProperty prop = obj.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(true)) {
            if (prop.name == "isWalkable" && currentTileSize.x != 1 && currentTileSize.y != 1) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(prop);
                prop.boolValue = false;
                EditorGUI.EndDisabledGroup();
            } else {
                EditorGUILayout.PropertyField(prop);
            }
        }

        if (obj.ApplyModifiedProperties()) {
            updateTilePrefab();
        }
    }

    /* Draws the preview image of the current sprite.
     */
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
        drawSprite(previewRect, currentTile.groundSprite, _spriteAngle);
        drawSprite(previewRect, currentTile.groundEffectSprite, _spriteAngle);
        drawSprite(previewRect, currentTile.objectSprite, _spriteAngle);
        drawSprite(previewRect, currentTile.overlaySprite, _spriteAngle);
    }

    private void displayTransformControls() {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rotate Left")) {
            _spriteAngle -= 90.0f;
        }
        if (GUILayout.Button("Rotate right")) {
            _spriteAngle += 90.0f;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    /* Displays the Copy button, and handles the logic when a tile is copied.
     * Copied tiles get their own new name, and are immidiately selected.  
     * The copied prefab goes into the default Resources/TilePrefabs directory
     */
    private void displayCopyButton() {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create copy of tile")) {
            string newPrefabPath = "Assets/Resources/TilePrefabs/" + currentTilePrefab.name + " Copy.prefab";
            string newPrefabPath2 = "TilePrefabs/" + currentTilePrefab.name + " Copy";
            PrefabUtility.CreatePrefab(newPrefabPath, currentTilePrefab);
            currentTilePrefab = Resources.Load<GameObject>(newPrefabPath2);
            refreshTilePrefabList();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    /* Displays the reset button.  Pressing this button clears the entire tilemap.
     * There is a confirmation menu that is displayed to ensure that the user
     * does not destroy the tilemap by accident
     */
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

    /* This method handles the painting of tiles onto the tilemap
     */
    public void OnSceneGUI() {
        int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        mouseHighlight();

        switch (Event.current.type) {
            case EventType.mouseDown:
                mouseDown();
                break;
            case EventType.mouseDrag:
                mouseDrag();
                break;
            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            default:
                break;
        }
    }

    private void drawSprite(Rect rect, Sprite sprite, float angle = 0.0f) {
        if (sprite) {
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, rect.position + rect.size / 2.0f);
            GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit);
            GUI.matrix = matrixBackup;
        }
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

    private void mouseHighlight() {
        if(currentTilePrefab != null){
            Tile currentTile = currentTilePrefab.GetComponent<Tile>();
            Vector2 intersection;
            if(getTilemapIntersection(Event.current.mousePosition, out intersection)){
                Vector2Int tilePos = intersection;
                intersection = new Vector2(tilePos.x * Tilemap.TILE_SIZE, tilePos.y * Tilemap.TILE_SIZE);

                intersection -= (Vector2.up + Vector2.right) / 2.0f;
                Vector2Int size = currentTile.getTileSize();

                Handles.color = Color.blue;
                Handles.DrawAAPolyLine(15.0f, intersection, intersection + Vector2.right * size.x);
                Handles.DrawAAPolyLine(15.0f, intersection, intersection + Vector2.up * size.y);
                Handles.DrawAAPolyLine(15.0f, intersection + Vector2.right * size.x, intersection + Vector2.up * size.y + Vector2.right * size.x);
                Handles.DrawAAPolyLine(15.0f, intersection + Vector2.up * size.y, intersection + Vector2.up * size.y + Vector2.right * size.x);
                HandleUtility.Repaint();
            }
        }
    }

    private void mouseDrag() {
        if (Event.current.button == 0 && currentTilePrefab) {
            Tile currentTile = currentTilePrefab.GetComponent<Tile>();
            Vector2Int currentTileSize = currentTile.getTileSize();
            if (currentTileSize.x != 1 && currentTileSize.y != 1) {
                return;
            }

            handleMouseDraw(Event.current.mousePosition - Event.current.delta, Event.current.mousePosition);
        }
    }

    private void handleMouseDraw(Vector2 start, Vector2 end) {
        Vector2 intersectionStart;
        if (getTilemapIntersection(start, out intersectionStart)) {
            Vector2 intersectionEnd;
            if (getTilemapIntersection(end, out intersectionEnd)) {
                Vector2Int startPosition = intersectionStart;
                Vector2Int endPosition = intersectionEnd;
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
        Undo.RegisterCreatedObjectUndo(newTileObject, "Added new tiles");
        Undo.RecordObject(newTileObject, "asdads");
        newTileObject.transform.eulerAngles = new Vector3(0, 0, -_spriteAngle);
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
