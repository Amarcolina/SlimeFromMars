using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SpriteRenderer))]
public class Tile : MonoBehaviour {
    public string tester;
    /*
    private GameObject _floorSprite;
    private GameObject _objectSprite;
    private GameObject _overlaySprite;
    private TileSpecification _specification;

    public void setSpecification(TileSpecification specification) {
        _specification = specification;
        updateSpriteObject(ref _floorSprite, specification.floorSprite, "FloorSprite", 0.0f);
        updateSpriteObject(ref _objectSprite, specification.objectSprite, "ObjectSprite", 0.1f);
        updateSpriteObject(ref _overlaySprite, specification.overlaySprite, "OverlaySprite", 0.2f);
    }

    private void updateSpriteObject(ref GameObject obj, Sprite sprite, string name, float zOffset) {
        if (sprite == null) {
            if (obj != null) {
                DestroyImmediate(obj);
            }
        }else{
            if (obj == null) {
                obj = new GameObject(name);
                obj.transform.parent = transform;
                obj.AddComponent<SpriteRenderer>();
            }
            Vector3 pos = obj.transform.position;
            pos.z = zOffset;
            obj.transform.position = pos;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
        }
    }
     * */
}
