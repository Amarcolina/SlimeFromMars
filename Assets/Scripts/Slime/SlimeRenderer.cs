using UnityEngine;
using System.Collections;

public delegate float IsCellSolidFunction(Vector2Int cellPosition);

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
    private float _morphTime = 0.2f;

    private float[] _cellSolidity = new float[9];
    
    private Vector2Int _rendererPosition;
    private SpriteRenderer _spriteRenderer;
    private Tilemap _tilemap;

    public void Awake() {
        _rendererPosition = Tilemap.getTilemapLocation(transform.position);
        _tilemap = Tilemap.getInstance();

        _solidityFunction = defaultSolidityFunction;

        GameObject rendererGameObject = new GameObject("SlimeRenderer");
        rendererGameObject.transform.parent = transform;
        rendererGameObject.transform.position = transform.position;
        _spriteRenderer = rendererGameObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sortingLayerName = "Slime";
        _spriteRenderer.enabled = false;

        if (GetComponent<Tile>().isSlimeable) {
            _spriteRenderer.material.shader = Shader.Find("Custom/SlimeShader");
        } else {
            _spriteRenderer.material.shader = Shader.Find("Custom/SlimeWallShader");
            _spriteRenderer.sortingLayerName = "Default";
            _spriteRenderer.sortingOrder = -1;
            _spriteRenderer.material.SetTexture("_Smear", Resources.Load<Texture>("Sprites/Slime/SlimeWallRamp"));
        }
    }

    public void OnDestroy() {
        Destroy(_spriteRenderer.gameObject);
    }

    public void setTextureRamp(Sprite ramp) {
        _spriteRenderer.sprite = ramp;
        _spriteRenderer.enabled = true;
    }

    public void setMorphTime(float morphTime) {
        _morphTime = morphTime;
    }

    public void setSolidityFunction(IsCellSolidFunction solidityFunction) {
        _solidityFunction = solidityFunction;
    }

    public float defaultSolidityFunction(Vector2Int position) {
        GameObject tileObject = _tilemap.getTileGameObject(position);
        if (tileObject == null) {
            return 0.0f;
        }

        Slime slime = tileObject.GetComponent<Slime>();

        if (slime == null) {
            return 0.0f;
        }

        return slime.isConnected() ? 1.0f : 0.92f;
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
            float isSolid = _solidityFunction(cellPosition);
            float newSolidity = Mathf.MoveTowards(_cellSolidity[i], isSolid, 0.01f);
            newSolidity += (isSolid - newSolidity) / 8.0f;
            soliditySum += newSolidity;
            if (newSolidity != _cellSolidity[i]) {
                _cellSolidity[i] = newSolidity;
                canFallAsleep = false;
            }
        }

        _spriteRenderer.material.SetVector("_weights0", new Vector4(_cellSolidity[0], _cellSolidity[1], _cellSolidity[2], _cellSolidity[3]));
        _spriteRenderer.material.SetVector("_weights1", new Vector4(_cellSolidity[4], _cellSolidity[5], _cellSolidity[6], _cellSolidity[7]));
        _spriteRenderer.material.SetFloat("_center", _cellSolidity[8]);

        updateNeighbors();

        if (canFallAsleep) {
            enabled = false;
            if (soliditySum == 0.0f  &&  _solidityFunction(_rendererPosition) == 0.0f) {
                Destroy(this);
            }
        }
    }

    private void updateNeighbors() {
        if(_solidityFunction(_rendererPosition) == 0.0f){
            return;
        }

        Tile myTile = GetComponent<Tile>();

        for (int i = 0; i < 8; i++) {
            Vector2Int neighborPosition = _rendererPosition + _cellOffset[i];
            GameObject tileObject = _tilemap.getTileGameObject(neighborPosition);
            if (tileObject != null) {

                if (!myTile.isTransparent && !tileObject.GetComponent<Tile>().isSlimeable) {
                    continue;
                }

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
                    newRenderer.setTextureRamp(_spriteRenderer.sprite);
                    newRenderer._morphTime = _morphTime;
                    newRenderer.setSolidityFunction(_solidityFunction);
                }
            }
        }
    }

    private bool isOfSameType(SlimeRenderer slimeRenderer) {
        return slimeRenderer._spriteRenderer.sprite == _spriteRenderer.sprite;
    }
}
