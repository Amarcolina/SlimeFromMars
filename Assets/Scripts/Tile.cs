using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SpriteRenderer))]
public class Tile : MonoBehaviour {
    public bool isWalkable;
    public Sprite groundSprite = null;
    public Sprite objectSprite = null;
    public Sprite effectSprite = null;
    public Sprite overlaySprite = null;
    
    private int _tileX, _tileY;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
