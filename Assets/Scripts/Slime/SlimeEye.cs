using UnityEngine;
using System.Collections;

public class SlimeEye : MonoBehaviour {
    // Use this for initialization
    SlimeController slime;
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        /*
        if (slime.getSelectedSlime() == null) {
            renderer.enabled = false;
        }
         */
        movePupil();
    }

    public void movePupil() {
       // if(transform.localPosition){
         //   transform.localPosition = new Vector2(0, 0);
        //} 
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.localPosition = mousePos;
    }
}
