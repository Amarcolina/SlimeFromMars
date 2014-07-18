using UnityEngine;
using System.Collections;

public class ScreenScroller : MonoBehaviour {
    int scrollDistance;
    float scrollSpeed;
    // Use this for initialization
    void Start() {
        scrollDistance = 1;
        scrollSpeed = 70;
    }

    // Update is called once per frame
    void Update() {
        var mousePosX = Input.mousePosition.x;
        var mousePosY = Input.mousePosition.y;
       
        if (mousePosX < scrollDistance) {
            transform.Translate(Vector3.right * -scrollSpeed * Time.deltaTime);
        }

        if (mousePosX >= Screen.width - scrollDistance) {
            transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
        }

        if (mousePosY < scrollDistance) {
            transform.Translate(transform.forward * -scrollSpeed * Time.deltaTime);
        }

        if (mousePosY >= Screen.height - scrollDistance) {
            transform.Translate(transform.forward * scrollSpeed * Time.deltaTime);
        }

    }
}
