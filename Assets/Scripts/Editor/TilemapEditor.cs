using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (Tilemap))]
public class TilemapEditor : Editor {
    TileSpecification currentSpecification;


    public override void OnInspectorGUI() {
        base.OnInspectorGUI();


        if (currentSpecification == null) {
            currentSpecification = ScriptableObject.CreateInstance<TileSpecification>();
        }

        EditorGUILayout.LabelField("Current Tile");
        SerializedObject obj = new SerializedObject(currentSpecification);
        SerializedProperty prop = obj.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(true)) {
            EditorGUILayout.PropertyField(prop);
        }
        obj.ApplyModifiedProperties();

        if (currentSpecification.floorSprite) {
            GUILayout.Box(currentSpecification.floorSprite.texture);
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

    }

    private void mouseMove() {

    }

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
                                tilemap.setTileGameObject(new TilemapOffset(x, y), null);
                            }
                            prevX = x;
                            prevY = y;
                            set = true;
                        }
                    } else {
                        tilemap.setTileGameObject(endPosition, null);
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
}
