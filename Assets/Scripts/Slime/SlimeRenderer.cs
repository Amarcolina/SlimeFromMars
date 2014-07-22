using UnityEngine;
using System.Collections;

public delegate bool IsCellSolidFunction(Vector2Int cellPosition);

public class SlimeRenderer : MonoBehaviour {
    private static Vector2Int[] _cellOffset = {new Vector2Int(-1, -1),
                                               new Vector2Int( 0, -1),
                                               new Vector2Int( 1, -1),
                                               new Vector2Int( 1,  0),
                                               new Vector2Int( 1,  1),
                                               new Vector2Int( 0,  1),
                                               new Vector2Int(-1,  1),
                                               new Vector2Int(-1,  0),
                                               new Vector2Int( 0,  0)};

    private float[] _cellSolidity = new float[9];
    private Texture2D _distanceRamp = null;
    private IsCellSolidFunction _solidityFunction = null;
    private Texture2D _texture = null;
    private float _morphSpeed = 0.1f;
    private Vector2Int _rendererPosition;
    private SpriteRenderer _spriteRenderer;
    private Tilemap _tilemap;

    public void Awake() {
        _texture = new Texture2D(Tile.TILE_PIXEL_SIZE, Tile.TILE_PIXEL_SIZE, TextureFormat.ARGB32, false);
        _rendererPosition = Tilemap.getTilemapLocation(transform.position);
        _tilemap = Tilemap.getInstance();

        GameObject rendererGameObject = new GameObject("SlimeRenderer");
        rendererGameObject.transform.parent = transform;
        rendererGameObject.transform.position = transform.position;
        _spriteRenderer = rendererGameObject.AddComponent<SpriteRenderer>();

        _spriteRenderer.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
    }

    public void OnDestroy() {
        Destroy(_spriteRenderer.gameObject);
    }

    public void setSolidityFunction(IsCellSolidFunction solidityFunction) {
        _solidityFunction = solidityFunction;
    }

    public void setDistanceRamp(Texture2D distanceRamp) {
        _distanceRamp = distanceRamp;
    }

    public void setMorphSpeed(float morphSpeed) {
        _morphSpeed = morphSpeed;
    }

    public void wakeUpRenderer() {
        enabled = true;
    }

    public void Update() {
        bool canFallAsleep = true;
        float soliditySum = 0.0f;

        for (int i = 0; i < _cellSolidity.Length; i++) {
            Vector2Int cellPosition = _cellOffset[i] + _rendererPosition;
            float isSolid = _solidityFunction(cellPosition) ? 1.0f : 0.0f;
            float newSolidity = Mathf.MoveTowards(_cellSolidity[i], isSolid, _morphSpeed * Time.deltaTime);
            if (newSolidity != _cellSolidity[i]) {
                _cellSolidity[i] = newSolidity;
                soliditySum += newSolidity;
                canFallAsleep = false;
            }
        }

        updateNeighbors();
        updateTexture();

        if (canFallAsleep) {
            enabled = false;
            if (soliditySum == 0.0f) {
                Destroy(this);
            }
        }
    }

    private void updateNeighbors() {
        if(!_solidityFunction(_rendererPosition)){
            return;
        }
        for (int i = 0; i < 8; i++) {
            Vector2Int neighborPosition = _rendererPosition + _cellOffset[i];
            GameObject tileObject = _tilemap.getTileGameObject(neighborPosition);
            if (tileObject != null) {
                SlimeRenderer[] renderers = tileObject.GetComponents<SlimeRenderer>();
                bool hasSameRenderer = false;
                foreach (SlimeRenderer otherRenderer in renderers) {
                    if (isOfSameType(otherRenderer)) {
                        hasSameRenderer = true;
                        break;
                    }
                }

                if (!hasSameRenderer) {
                    SlimeRenderer newRenderer = tileObject.AddComponent<SlimeRenderer>();
                    newRenderer.setDistanceRamp(_distanceRamp);
                    newRenderer.setMorphSpeed(_morphSpeed);
                    newRenderer.setSolidityFunction(_solidityFunction);
                }
            }
        }
    }

    private bool isOfSameType(SlimeRenderer slimeRenderer) {
        return slimeRenderer._solidityFunction == _solidityFunction &&
               slimeRenderer._distanceRamp == _distanceRamp;
    }

    private void updateTexture() {
        for (int x = 0; x < _texture.width; x++) {
            for (int y = 0; y < _texture.height; y++) {
                //float pointX = (x - _texture.width / 2.0f) / _texture.width;
                //float pointY = (y - _texture.height / 2.0f) / _texture.height;
                //Vector2 point = new Vector2(x, y);
                //Color c = getPointColor(point);
                Color c = Color.green;
                _texture.SetPixel(x, y, c);
            }
        }
    }

    private Color getPointColor(Vector2 point) {
        float pointValue = calculatePointValue(point);
        int rampValue = (int)(pointValue * _distanceRamp.height);
        return _distanceRamp.GetPixel(0, rampValue);
    }

    private float calculatePointValue(Vector2 point) {
        float pointValue = 1.0f;
        for (int i = 0; i < _cellOffset.Length; i++) {
            Vector2Int cellPosInt = _rendererPosition + _cellOffset[i];
            Vector2 cellPos = new Vector2(cellPosInt.x, cellPosInt.y);
            pointValue = Mathf.Min(pointValue, distanceField(Vector2.zero, cellPos, point));
        }

        for (int i = 1; i < _cellOffset.Length; i += 2) {
            Vector2Int cellPosInt0 = _rendererPosition + _cellOffset[i];
            Vector2 cellPos0 = new Vector2(cellPosInt0.x, cellPosInt0.y);

            Vector2Int cellPosInt1 = _rendererPosition + _cellOffset[(i+2)%_cellOffset.Length];
            Vector2 cellPos1 = new Vector2(cellPosInt1.x, cellPosInt1.y);

            pointValue = Mathf.Min(pointValue, distanceField(cellPos0, cellPos1, point));
        }

        return pointValue;
    }

    private float distanceField(Vector2 seg0, Vector2 seg1, Vector2 point) {
        float t = Mathf.Clamp01(Vector2.Dot(point - seg0, seg1 - seg0) / Vector2.SqrMagnitude(seg0 - seg1));
        return ((seg1 - seg0) * t + seg0 - point).magnitude;
    }
}
