using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {
    public const int TEXTURE_SIZE = 512;
    public const int TEXTURE_OFFSET_X = -128;
    public const int TEXTURE_OFFSET_Y = -64;
    public const int MINIMAP_ZOOM = 8;

    private Texture2D _levelTexture;
    private Texture2D _fogTexture;
    private UITexture _guiTexture;

    private bool _isFillingLevelTexture = true;
    private int _initRow = 0;
    private ScreenScroller _screenScroller;

    private Rect minimapCamera = new Rect();


    private bool _isDraggingCamera = false;

    public void Awake() {
        _levelTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false, true);
        _fogTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false, true);
        _guiTexture = GetComponent<UITexture>();

        _guiTexture.mainTexture = _levelTexture;
        _guiTexture.pivot = UIWidget.Pivot.BottomRight;

        _screenScroller = Camera.main.GetComponent<ScreenScroller>();
    }

    public void Update() {
        if (_isFillingLevelTexture) {
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
            Vector2 newCamera = new Vector2(minimapCamera.x * 512 + TEXTURE_OFFSET_X, minimapCamera.y * 512 + TEXTURE_OFFSET_Y) + 
                                new Vector2(mapMouse.x * minimapCamera.width * 512, (1 - mapMouse.y) * minimapCamera.height * 512);
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

        //float x = (transform.position.x - TEXTURE_OFFSET_X + 0.5f) / TEXTURE_SIZE;
        //float y = (transform.position.y + 0.5f) / TEXTURE_SIZE;

        //float height = camera.orthographicSize / TEXTURE_SIZE;
        //float width = height / Screen.height * Screen.width;
        


        //GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _levelTexture, new Rect(x - width, y - height, width * 2, height * 2));
}
