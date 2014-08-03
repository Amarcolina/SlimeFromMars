using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {
    public const int TEXTURE_SIZE = 512;
    public const int TEXTURE_OFFSET_X = -128;
    public const int TEXTURE_OFFSET_Y = -64;
    public const int MINIMAP_ZOOM = 24;

    public UITexture fogOfWarUITexture;
    public UITexture fogOfWarMinimapUITexture;
    public UITexture minimapUITexture;

    private Texture2D _levelTexture;
    private Texture2D _fogTexture;

    private bool _isFillingLevelTexture = true;
    private int _initRow = 0;

    private ScreenScroller _screenScroller;
    private Rect minimapCamera = new Rect();

    private bool _isDraggingCamera = false;

    private static Minimap _instance = null;
    public static Minimap getInstance() {
        if (_instance == null) {
            _instance = FindObjectOfType<Minimap>();
        }
        return _instance;
    }

    public void Awake() {
        _levelTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false, true);
        _levelTexture.filterMode = FilterMode.Point;
        _levelTexture.wrapMode = TextureWrapMode.Clamp;
        _fogTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.ARGB32, false, true);
        _fogTexture.wrapMode = TextureWrapMode.Clamp;

        minimapUITexture.mainTexture = _levelTexture;
        fogOfWarUITexture.mainTexture = _fogTexture;
        fogOfWarMinimapUITexture.mainTexture = _fogTexture;

        for (int i = 0; i < _fogTexture.width; i++) {
            for (int j = 0; j < _fogTexture.height; j++) {
                _fogTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        _fogTexture.Apply();

        _screenScroller = Camera.main.GetComponent<ScreenScroller>();
    }

    public void Update() {
        if (_isFillingLevelTexture) {
            fillLevelTexture();
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            fogOfWarMinimapUITexture.enabled = false;
            fogOfWarUITexture.enabled = false;
        }

        handleMinimap();
    }

    public void LateUpdate() {
        handleFogOfWar();
    }

    public bool isPositionInFogOfWar(Vector2Int position) {
        Color c = _fogTexture.GetPixel(position.x - TEXTURE_OFFSET_X, position.y - TEXTURE_OFFSET_Y);
        return c.a > 0.9f;
    }

    public void clearFogOfWar(Vector2Int center, int envelopeRadius, int maxRadius) {
        for (int dx = -maxRadius; dx <= maxRadius; dx++) {
            for (int dy = -maxRadius; dy <= maxRadius; dy++) {
                int distSqrd = dx * dx + dy * dy;
                if (distSqrd > maxRadius * maxRadius) {
                    continue;
                }
                float dist = Mathf.Sqrt(distSqrd);
                float percent = (dist - envelopeRadius) / (float)(maxRadius - envelopeRadius);
                float curr = _fogTexture.GetPixel(center.x + dx - TEXTURE_OFFSET_X, center.y + dy - TEXTURE_OFFSET_Y).a;
                if(percent < curr){
                    _fogTexture.SetPixel(center.x + dx - TEXTURE_OFFSET_X, center.y + dy - TEXTURE_OFFSET_Y, new Color(0, 0, 0, percent));
                }
            }
        }
        _fogTexture.Apply();
    }

    private void handleFogOfWar() {
        float height = Camera.main.orthographicSize * 2.0f / 512.0f;
        float width = Screen.width * height / Screen.height;

        float x = (Camera.main.transform.position.x - TEXTURE_OFFSET_X) / 512.0f;
        float y = (Camera.main.transform.position.y - TEXTURE_OFFSET_Y) / 512.0f;

        fogOfWarUITexture.uvRect = new Rect(x - width / 2.0f, y - height / 2.0f, width, height);
    }

    private void handleMinimap() {
        Vector2 mapMouse = Input.mousePosition;
        mapMouse.y = Screen.height - mapMouse.y;
        mapMouse -= new Vector2(Screen.width * (1.0f - 0.2f), Screen.height * (1.0f - 0.2f));
        mapMouse.x /= Screen.width * 0.2f;
        mapMouse.y /= Screen.height * 0.2f;

        if (Input.GetMouseButtonDown(0)) {
            if (mapMouse.x > 0 && mapMouse.y > 0) {
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
            minimapCamera.height = minimapCamera.width * minimapUITexture.transform.localScale.y / minimapUITexture.transform.localScale.x;

            minimapCamera.x = (Camera.main.transform.position.x - TEXTURE_OFFSET_X) / TEXTURE_SIZE - minimapCamera.width / 2.0f;
            minimapCamera.y = (Camera.main.transform.position.y - TEXTURE_OFFSET_Y) / TEXTURE_SIZE - minimapCamera.height / 2.0f;

            minimapUITexture.uvRect = minimapCamera;
            fogOfWarMinimapUITexture.uvRect = minimapCamera;
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
