using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SpriteRenderer))]
public class Tile : MonoBehaviour {
    private const string GROUND_LAYER_NAME = "TileGround";
    private const string OVERLAY_LAYER_NAME = "TileOverlay";

    public bool isWalkable = true;
    public bool isDeadly = false;
    public Sprite groundSprite;
    public Sprite groundEffectSprite;
    public Sprite objectSprite;
    public Sprite overlaySprite;

    private SpriteRenderer _groundSpriteRenderer;
    private SpriteRenderer _groundEffectSpriteRenderer;
    private SpriteRenderer _objectSpriteRenderer;
    private SpriteRenderer _overlaySpriteRenderer;

    public void updateTileWithSettings() {
        _groundSpriteRenderer = GetComponent<SpriteRenderer>();
        _groundSpriteRenderer.sprite = groundSprite;
        _groundSpriteRenderer.sortingLayerName = "TileGround";

        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if (child != transform) {
                DestroyImmediate(child.gameObject);
            }
        }

        _groundEffectSpriteRenderer = createRenderer("GroundEffect", groundEffectSprite, "TileGroundEffect");
        _objectSpriteRenderer = createRenderer("Object", objectSprite, "TileObject");
        _overlaySpriteRenderer = createRenderer("Overlay", overlaySprite, "TileOverlay");
    }

    private SpriteRenderer createRenderer(string objName, Sprite sprite, string layerName) {
        if (sprite) {
            GameObject rendererObject = new GameObject(objName);
            rendererObject.transform.parent = transform;
            rendererObject.transform.position = transform.position;
            SpriteRenderer spriteRenderer = rendererObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingLayerName = layerName;
            return spriteRenderer;
        }
        return null;
    }
}
