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

    private IsCellSolidFunction _solidityFunction = null;
    private Texture2D _textureRamp = null;
    private float _morphTime = 0.2f;

    private float[] _cellSolidity = new float[9];
    private Texture2D _texture = null;
    
    private Vector2Int _rendererPosition;
    private SpriteRenderer _spriteRenderer;
    private Tilemap _tilemap;

    public void Awake() {
        _texture = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;

        _rendererPosition = Tilemap.getTilemapLocation(transform.position);
        _tilemap = Tilemap.getInstance();

        _solidityFunction = defaultSolidityFunction;

        GameObject rendererGameObject = new GameObject("SlimeRenderer");
        rendererGameObject.transform.parent = transform;
        rendererGameObject.transform.position = transform.position;
        _spriteRenderer = rendererGameObject.AddComponent<SpriteRenderer>();

        _spriteRenderer.enabled = false;
        _spriteRenderer.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), _texture.width);
    }

    public void OnDestroy() {
        Destroy(_spriteRenderer.gameObject);
    }

    public void setTextureRamp(Texture2D textureRamp) {
        if (textureRamp == null) {
            Debug.Log("super baaaaaaad");
        }
        _textureRamp = textureRamp;
    }

    public void setMorphTime(float morphTime) {
        _morphTime = morphTime;
    }

    public void setSolidityFunction(IsCellSolidFunction solidityFunction) {
        _solidityFunction = solidityFunction;
    }

    public bool defaultSolidityFunction(Vector2Int position) {
        GameObject tileObject = _tilemap.getTileGameObject(position);
        if (tileObject == null) {
            return false;
        }
        return tileObject.GetComponent<Slime>() != null;
    }

    public void wakeUpRenderer(bool wakeUpNeighbors = true) {
        if (wakeUpNeighbors) {
            for (int i = 0; i < 8; i++) {
                Vector2Int neighborPos = _rendererPosition + _cellOffset[i];
                GameObject obj = _tilemap.getTileGameObject(neighborPos);
                if (obj != null) {
                    SlimeRenderer otherRenderer = obj.GetComponent<SlimeRenderer>();
                    if (otherRenderer != null) {
                        otherRenderer.wakeUpRenderer(false);
                    }
                }
            }
        }
        enabled = true;
    }

    public void Update() {
        bool canFallAsleep = true;
        float soliditySum = 0.0f;

        for (int i = 0; i < _cellSolidity.Length; i++) {
            Vector2Int cellPosition = _cellOffset[i] + _rendererPosition;
            float isSolid = _solidityFunction(cellPosition) ? 1.0f : 0.0f;
            float newSolidity = Mathf.MoveTowards(_cellSolidity[i], isSolid, 0.1f);//Time.deltaTime / _morphTime);
            soliditySum += newSolidity;
            if (newSolidity != _cellSolidity[i]) {
                _cellSolidity[i] = newSolidity;
                canFallAsleep = false;
            }
        }

        updateNeighbors();
        updateTexture();

        if (canFallAsleep) {
            enabled = false;
            if (soliditySum == 0.0f  &&  !_solidityFunction(_rendererPosition)) {
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
                    newRenderer.setTextureRamp(_textureRamp);
                    newRenderer._morphTime = _morphTime;
                    newRenderer.setSolidityFunction(_solidityFunction);
                }
            }
        }
    }

    private bool isOfSameType(SlimeRenderer slimeRenderer) {
        return slimeRenderer._textureRamp == _textureRamp;
    }

    private void updateTexture() {
        for (int x = 0; x < _texture.width; x++) {
            for (int y = 0; y < _texture.height; y++) {
                float pointX = (x - _texture.width / 2.0f) / _texture.width;
                float pointY = (y - _texture.height / 2.0f) / _texture.height;
                Vector2 point = new Vector2(pointX, pointY);
                Color c = getPointColor(point);
                _texture.SetPixel(x, y, c);
            }
        }
        _texture.Apply();
        _spriteRenderer.enabled = true;
    }

    private Color getPointColor(Vector2 point) {
        float minDistance = 1.0f - calculateMinDistance(point) * 2.0f;
        float rampValue = Mathf.Pow(minDistance * 0.85f, 4.0f) * 8.0f;
        return _textureRamp.GetPixelBilinear(0.0f, rampValue);
    }

    private float calculateMinDistance(Vector2 point) {
        float minDistance = 0.5f;

        if (_cellSolidity[8] > 0) {
            for (int i = 0; i < _cellOffset.Length; i++) {
                Vector2Int cellPosInt = _cellOffset[i];
                Vector2 cellPos = new Vector2(cellPosInt.x, cellPosInt.y);
                minDistance = distanceToSegment(minDistance, Vector2.zero, _cellSolidity[8], cellPos, _cellSolidity[i], point);
            }
        }

        for (int i = 1; i < 8; i += 2) {
            int index0 = i;
            int index1 = (i + 2) % 8;

            if (_cellSolidity[index0] > 0.0f && _cellSolidity[index1] > 0.0f) {
                Vector2Int cellPosInt0 = _cellOffset[index0];
                Vector2 cellPos0 = new Vector2(cellPosInt0.x, cellPosInt0.y);

                Vector2Int cellPosInt1 = _cellOffset[index1];
                Vector2 cellPos1 = new Vector2(cellPosInt1.x, cellPosInt1.y);

                minDistance = distanceToSegment(minDistance, cellPos0, _cellSolidity[index0], cellPos1, _cellSolidity[index1], point);
            }
        }

        return minDistance;
    }

    private float smoothMin(float a, float b) {
        float k = 0.22f;
        float h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
        return Mathf.Lerp(b, a, h) - k * h * (1.0f - h);
    }

    private float distanceToSegment(float curr, Vector2 seg0, float scale0, Vector2 seg1, float scale1, Vector2 point) {
        if (scale0 == 0.0f || scale1 == 0.0f) {
            return curr;
        }

        Vector2 delta = seg1 - seg0;
        float mag2 = delta.sqrMagnitude;
        if (mag2 <= 0.0f) {
            return smoothMin(curr, Mathf.Lerp(1.0f, (point - seg0).magnitude, scale0));
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - seg0, seg1 - seg0) / mag2);

        return smoothMin(curr, Mathf.Lerp(1.0f, (delta * t + seg0 - point).magnitude, scale0 * scale1));
    }
}
