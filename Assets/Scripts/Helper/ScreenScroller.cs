using UnityEngine;
using System.Collections;

public class ScreenScroller : MonoBehaviour {
    [MinValue(0)]
    public int scrollDistance = 1;
    [MinValue(0)]
    public float scrollSpeed = 70;

    private float _goalZoom;

    public void Awake() {
        _goalZoom = camera.orthographicSize;
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

        _goalZoom -= Input.GetAxis("Mouse ScrollWheel") * 4;
        _goalZoom = Mathf.Clamp(_goalZoom, 5, 10);
        camera.orthographicSize += (_goalZoom - camera.orthographicSize) / 5.0f;

    }
}
