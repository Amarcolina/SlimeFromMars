using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (Tilemap))]
public class TilemapEditor : Editor {
    private float tileSize = 10;

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



        /*


        Vector2 mouse = Event.current.mousePosition;
        
        Handles.color = Color.blue;
        
            

            //GUILayout.Box(tex);


            //Handles.CubeCap(0, new Vector3(tileX, tileY, 0), Quaternion.identity, 1);
            //Handles.CubeCap(0, intersection, Quaternion.identity, 10);
        }

        //GUILayout.Box()

        //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
        Event.current.Use();
        //if(Event.current.isMouse)
        //Handles.DrawLine(mouse, mouse + new Vector2(64, 64));
        //Rect rect = new Rect(mouse.x, mouse.y, 64, 64);

        //Handles.BeginGUI();
        //EditorGUI.DrawRect(rect, Color.blue);
        //Handles.EndGUI();

        //SceneView.RepaintAll();
         * */
    }

    private void mouseDown() {

    }

    private void mouseMove() {

    }

    private void mouseDrag() {

    }

    /*
    private bool getTileMouseRect(out Vector2 tileOffset) {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane tilemapPlane = new Plane(Vector3.back, Vector3.zero);
        float dist = 0;
        if (tilemapPlane.Raycast(mouseRay, out dist)) {
            Vector3 intersection = mouseRay.origin + mouseRay.direction * dist;
            int tileX = (int)System.Math.Round(intersection.x / tileSize, System.MidpointRounding.ToEven);
            int tileY = (int)System.Math.Round(intersection.y / tileSize, System.MidpointRounding.ToEven);
            mouseRect = new Rect(tileX - tileSize, tileY - tileSize, tileSize, tileSize);
            return true;
        }
        return false;
    }
     */
}
