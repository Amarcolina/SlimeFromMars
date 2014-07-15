using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (Tilemap))]
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

        GameObject newTile = (GameObject) EditorGUILayout.ObjectField("Current Tile", currentTilePrefab, typeof(GameObject), false);
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
            obj.ApplyModifiedProperties();

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
            drawSprite(previewRect, currentTile.objectEffectSprite);
            drawSprite(previewRect, currentTile.overlaySprite);

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (GUILayout.Button("Create copy of tile")) {
                string newPrefabPath = "Assets/Resources/Tiles/" + currentTilePrefab.name + " Copy.prefab";
                string newPrefabPath2 = "Tiles/" + currentTilePrefab.name + " Copy";
                PrefabUtility.CreatePrefab(newPrefabPath, currentTilePrefab);
                currentTilePrefab = Resources.Load<GameObject>(newPrefabPath2);
                Debug.Log(currentTilePrefab);
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
                mouseMove();
                break;
            case EventType.MouseDrag:
                //mouseDrag();
                break;
            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            default:
                break;
        }
    }

    private void mouseDown() {

    }

    private void mouseMove() {

    }

    /*
    private void mouseDrag() {
        if (Event.current.button == 0) {
            Tilemap tilemap = target as Tilemap;
            Vector2 intersectionStart;
            if (getTilemapIntersection(Event.current.mousePosition - Event.current.delta, out intersectionStart)) {
                Vector2 intersectionEnd;
                if (getTilemapIntersection(Event.current.mousePosition, out intersectionEnd)) {
                    TilemapOffset startPosition = Tilemap.getTilemapOffset(intersectionStart);
                    TilemapOffset endPosition = Tilemap.getTilemapOffset(intersectionEnd);
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
                                tilemap.setTileGameObject(new TilemapOffset(x, y), currentSpecification);
                            }
                            prevX = x;
                            prevY = y;
                            set = true;
                        }
                    } else {
                        tilemap.setTileGameObject(endPosition, currentSpecification);
                    }
                    
                    Event.current.Use();
                }
            }
        }
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
     * */
}
