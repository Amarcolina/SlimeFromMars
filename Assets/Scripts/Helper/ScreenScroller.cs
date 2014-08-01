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

        float scrollSpeedAdjusted = scrollSpeed * camera.orthographicSize / 5.0f;
        if (mousePosX < scrollDistance) {
            transform.Translate(Vector3.right * -scrollSpeedAdjusted * Time.deltaTime);
        }

        if (mousePosX >= Screen.width - scrollDistance) {
            transform.Translate(Vector3.right * scrollSpeedAdjusted * Time.deltaTime);
        }

        if (mousePosY < scrollDistance) {
            transform.Translate(transform.up * -scrollSpeedAdjusted * Time.deltaTime);
        }

        if (mousePosY >= Screen.height - scrollDistance) {
            transform.Translate(transform.up * scrollSpeedAdjusted * Time.deltaTime);
        }

        _goalZoom -= Input.GetAxis("Mouse ScrollWheel") * camera.orthographicSize;
        _goalZoom = Mathf.Clamp(_goalZoom, 5, 50);
        camera.orthographicSize += (_goalZoom - camera.orthographicSize) / 5.0f;

    }
}
