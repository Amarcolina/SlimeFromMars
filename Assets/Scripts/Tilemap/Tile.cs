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

    private Sprite _combinedGroundSprite;

    private SpriteRenderer _groundRenderer;
    private GameObject _overlayObject;
    private SpriteRenderer _overlayRenderer;

    public void Start() {
        updateTileWithSettings();

        if (overlaySprite) {
            _overlayObject = new GameObject("Tile Overlay");
            _overlayRenderer = _overlayObject.AddComponent<SpriteRenderer>();
            _overlayRenderer.sortingLayerName = OVERLAY_LAYER_NAME;
        }
    }

    public void updateTileWithSettings() {
        _groundRenderer = GetComponent<SpriteRenderer>();
        _groundRenderer.sortingLayerName = GROUND_LAYER_NAME;
        _combinedGroundSprite = TextureCombiner.combineTextures(groundSprite, groundEffectSprite, objectSprite);
        _groundRenderer.sprite = _combinedGroundSprite;
    }
}
