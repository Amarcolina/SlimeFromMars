using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenScroller : MonoBehaviour, ISaveable {
    [MinValue(0)]
    public int scrollDistance = 1;
    [MinValue(0)]
    public float scrollSpeed = 70;
    [MinValue(0)]
    public float minimapScale = 3;

    private float _goalZoom;
    private Camera _minimapCamera = null;

    public void Awake() {
        _goalZoom = camera.orthographicSize;

        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera c in cameras) {
            if (c != camera) {
                _minimapCamera = c;
                break;
            }
        }
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
        _minimapCamera.orthographicSize = camera.orthographicSize * minimapScale;
    }

    public void onSave(SavedComponent data) {
        data.put(_goalZoom);
    }

    public void onLoad(SavedComponent data) {
        _goalZoom = (float)data.get();
        camera.orthographicSize = _goalZoom;    
    }
}
