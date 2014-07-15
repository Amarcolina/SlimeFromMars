using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SpriteRenderer))]
public class Tile : MonoBehaviour {
    public bool isWalkable = true;
    public bool isDeadly = false;
    public Sprite groundSprite;
    public Sprite groundEffectSprite;
    public Sprite objectSprite;
    public Sprite objectEffectSprite;
    public Sprite overlaySprite;

    private SpriteRenderer _groundRenderer;
    private GameObject _overlayObject;
    private SpriteRenderer _overlayRenderer;

    public void updateTileWithSettings() {
    }
}
