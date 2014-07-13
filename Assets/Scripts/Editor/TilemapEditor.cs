using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (Tilemap))]
public class TilemapEditor : Editor {

    public void OnSceneGUI() {
        
        Vector2 mouse = Event.current.mousePosition;
        
        Handles.color = Color.blue;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(mouse);
        Plane tilemapPlane = new Plane(Vector3.back, Vector3.zero);
        float dist = 0;
        if (tilemapPlane.Raycast(mouseRay, out dist)) {
            Vector3 intersection = mouseRay.origin + mouseRay.direction * dist;
            int tileX = (int)System.Math.Round(intersection.x, System.MidpointRounding.ToEven); ;
            int tileY = (int)System.Math.Round(intersection.y, System.MidpointRounding.ToEven); ;
            

            Handles.CubeCap(0, new Vector3(tileX, tileY, 0), Quaternion.identity, 1);





            //Handles.CubeCap(0, intersection, Quaternion.identity, 10);
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
        Event.current.Use();
        //if(Event.current.isMouse)
        //Handles.DrawLine(mouse, mouse + new Vector2(64, 64));
        //Rect rect = new Rect(mouse.x, mouse.y, 64, 64);

        //Handles.BeginGUI();
        //EditorGUI.DrawRect(rect, Color.blue);
        //Handles.EndGUI();

        SceneView.RepaintAll();
    }
}
