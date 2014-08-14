using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenScroller : MonoBehaviour, ISaveable {
    [MinValue(0)]
    public int scrollDistance = 1;
    [MinValue(0)]
    public float scrollSpeed = 70;

    private float _goalZoom;

    private Vector3 cameraPosition = Vector3.zero;
    private Vector3 cameraOffset = Vector3.zero;

    public void Awake() {
        _goalZoom = camera.orthographicSize;
        cameraPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        var mousePosX = Input.mousePosition.x;
        var mousePosY = Input.mousePosition.y;

        float scrollSpeedAdjusted = scrollSpeed * camera.orthographicSize / 5.0f;
        if (mousePosX < scrollDistance) {
            cameraPosition += Vector3.right * -scrollSpeedAdjusted * Time.deltaTime;
        }

        if (mousePosX >= Screen.width - scrollDistance) {
            cameraPosition += Vector3.right * scrollSpeedAdjusted * Time.deltaTime;
        }

        if (mousePosY < scrollDistance) {
            cameraPosition += transform.up * -scrollSpeedAdjusted * Time.deltaTime;
        }

        if (mousePosY >= Screen.height - scrollDistance) {
            cameraPosition += transform.up * scrollSpeedAdjusted * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            _goalZoom = 7.5f;
            Vector2 pos = SlimeController.getInstance().transform.position;
            cameraPosition = new Vector3(pos.x, pos.y, cameraPosition.z);
            cameraOffset = transform.position - cameraPosition;
        }

        _goalZoom -= Input.GetAxis("Mouse ScrollWheel") * camera.orthographicSize;
        _goalZoom = Mathf.Clamp(_goalZoom, 5, 50);
        camera.orthographicSize += (_goalZoom - camera.orthographicSize) / 5.0f;

        cameraOffset *= 0.82f;

        transform.position = cameraPosition + cameraOffset;
    }

    public void onSave(SavedComponent data) {
        data.put(_goalZoom);
    }

    public void onLoad(SavedComponent data) {
        _goalZoom = (float)data.get();
        camera.orthographicSize = _goalZoom;    
    }
}
