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

    public void Awake() {
        _levelTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false, true);
        _fogTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false, true);
        _guiTexture = GetComponent<UITexture>();

        _guiTexture.mainTexture = _levelTexture;
        _guiTexture.pivot = UIWidget.Pivot.BottomRight;
    }

    public void Update() {
        if (_isFillingLevelTexture) {
            Tilemap tilemap = Tilemap.getInstance();
            for (int x = 0; x < TEXTURE_SIZE; x++) {
                Tile tile = tilemap.getTile(new Vector2Int(x + TEXTURE_OFFSET_X, _initRow + TEXTURE_OFFSET_Y));
                if (tile != null) {
                    if (tile.isWalkable) {
                        _levelTexture.SetPixel(x, _initRow, new Color(1, 0, 0));
                    } else {
                        _levelTexture.SetPixel(x, _initRow, Color.black);
                    }
                    
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

        float cameraX = (Camera.main.transform.position.x - TEXTURE_OFFSET_X) / TEXTURE_SIZE;
        float cameraY = (Camera.main.transform.position.y - TEXTURE_OFFSET_Y) / TEXTURE_SIZE;

        float width = Camera.main.orthographicSize / TEXTURE_SIZE * MINIMAP_ZOOM;
        float height = width * transform.localScale.y / transform.localScale.x;

        _guiTexture.uvRect = new Rect(cameraX - width, cameraY - height, width * 2, height * 2);
    }

    public void OnGUI() {
        /*
        float x = (transform.position.x - TEXTURE_OFFSET_X) / TEXTURE_SIZE;
        float y = transform.position.y / TEXTURE_SIZE;

        float width = camera.orthographicSize / TEXTURE_SIZE * MINIMAP_ZOOM;
        float height = width * MINIMAP_HEIGHT / MINIMAP_WIDTH;


        GUI.DrawTextureWithTexCoords(new Rect(Screen.width - MINIMAP_WIDTH, Screen.height - MINIMAP_HEIGHT, MINIMAP_WIDTH, MINIMAP_HEIGHT), _levelTexture, new Rect(x - width, y - height, width * 2, height * 2));
         * */


        //float x = (transform.position.x - TEXTURE_OFFSET_X + 0.5f) / TEXTURE_SIZE;
        //float y = (transform.position.y + 0.5f) / TEXTURE_SIZE;

        //float height = camera.orthographicSize / TEXTURE_SIZE;
        //float width = height / Screen.height * Screen.width;
        


        //GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), _levelTexture, new Rect(x - width, y - height, width * 2, height * 2));
    }

}
