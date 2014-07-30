using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {
    public const int TEXTURE_SIZE = 512;
    public const int TEXTURE_OFFSET_X = -128;
    public const int TEXTURE_OFFSET_Y = -64;
    public const int MINIMAP_ZOOM = 8;

    private Texture2D _levelTexture;
    private UITexture _guiTexture;

    private bool _isFillingLevelTexture = true;
    private int _initRow = 0;

    private ScreenScroller _screenScroller;
    private Rect minimapCamera = new Rect();

    private bool _isDraggingCamera = false;

    public void Awake() {
        _levelTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false, true);
        _levelTexture.filterMode = FilterMode.Point;
        _guiTexture = GetComponent<UITexture>();

        _guiTexture.mainTexture = _levelTexture;
        _guiTexture.pivot = UIWidget.Pivot.BottomRight;

        _screenScroller = Camera.main.GetComponent<ScreenScroller>();
    }

    public void Update() {
        if (_isFillingLevelTexture) {
            fillLevelTexture();
        }

        Vector2 mapMouse = Input.mousePosition;
        mapMouse.y = Screen.height - mapMouse.y;
        mapMouse -= new Vector2(Screen.width * (1.0f - 0.2f), Screen.height * (1.0f - 0.2f));
        mapMouse.x /= Screen.width * 0.2f;
        mapMouse.y /= Screen.height * 0.2f;

        if (Input.GetMouseButtonDown(0)) {
            if(mapMouse.x > 0 && mapMouse.y > 0) {
                _isDraggingCamera = true;
                _screenScroller.enabled = false;
            }
        }
        if (Input.GetMouseButtonUp(0) && _isDraggingCamera) {
            _isDraggingCamera = false;
            _screenScroller.enabled = true;
        }



        if (_isDraggingCamera) {
            Vector2 newCamera = new Vector2(minimapCamera.x * TEXTURE_SIZE + TEXTURE_OFFSET_X, minimapCamera.y * TEXTURE_SIZE + TEXTURE_OFFSET_Y) +
                                new Vector2(mapMouse.x * minimapCamera.width * TEXTURE_SIZE, (1 - mapMouse.y) * minimapCamera.height * TEXTURE_SIZE);
            Camera.main.transform.position = newCamera;

            SlimeController.getInstance().skipNextFrame();
        } else {
            minimapCamera.width = Camera.main.orthographicSize / TEXTURE_SIZE * MINIMAP_ZOOM;
            minimapCamera.height = minimapCamera.width * transform.localScale.y / transform.localScale.x;

            minimapCamera.x = (Camera.main.transform.position.x - TEXTURE_OFFSET_X) / TEXTURE_SIZE - minimapCamera.width / 2.0f;
            minimapCamera.y = (Camera.main.transform.position.y - TEXTURE_OFFSET_Y) / TEXTURE_SIZE - minimapCamera.height / 2.0f;

            _guiTexture.uvRect = minimapCamera;
        }
    }

    private void fillLevelTexture() {
        Tilemap tilemap = Tilemap.getInstance();
        for (int x = 0; x < TEXTURE_SIZE; x++) {
            Tile tile = tilemap.getTile(new Vector2Int(x + TEXTURE_OFFSET_X, _initRow + TEXTURE_OFFSET_Y));
            if (tile != null) {
                _levelTexture.SetPixel(x, _initRow, tile.minimapColor);
            } else {
                _levelTexture.SetPixel(x, _initRow, Color.black);
            }
        }
        _levelTexture.Apply();
        _initRow++;
        if (_initRow == TEXTURE_SIZE) {
            _isFillingLevelTexture = false;
        }
    }
}
