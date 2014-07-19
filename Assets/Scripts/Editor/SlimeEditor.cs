using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Slime))]
public class SlimeEditor : Editor {

    public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        if (Application.isPlaying) {
            Slime slime = target as Slime;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(slime.isSolid() ? "Make soft" : "Make solid")) {
                slime.setSolid(!slime.isSolid());
            }
            if (GUILayout.Button("Damage")) {
                slime.damageSlime(0.2f);
            }
            if (GUILayout.Button("Kill")) {
                slime.damageSlime(1.0f);
            }
            GUILayout.EndHorizontal();
        }
    }
}
