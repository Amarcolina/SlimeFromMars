using UnityEngine;
using System.Collections;

public class EyeRenderController : MonoBehaviour {
    SlimeController slime;
    // Use this for initialization
    void Start() {
        slime = SlimeController.getInstance();
    }

    // Update is called once per frame
    void Update() {
        if (slime.getSelectedSlime() == null) {
            renderer.enabled = false;
        } else {
            renderer.enabled = true;
        }
    }
}
