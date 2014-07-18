using UnityEngine;
using System.Collections;

public class ScreenScroller : MonoBehaviour {
    [MinValue(0)]
    public int scrollDistance = 1;
    [MinValue(0)]
    public float scrollSpeed = 70;
    // Use this for initialization
    void Start() {
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
            transform.Translate(transform.up * -scrollSpeed * Time.deltaTime);
        }

        if (mousePosY >= Screen.height - scrollDistance) {
            transform.Translate(transform.up * scrollSpeed * Time.deltaTime);
        }

    }
}
