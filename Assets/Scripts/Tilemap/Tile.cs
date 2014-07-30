using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (SpriteRenderer))]
public class Tile : MonoBehaviour {
    public const int TILE_PIXEL_SIZE = 64;
    private const string GROUND_LAYER_NAME = "TileGround";
    private const string OVERLAY_LAYER_NAME = "TileOverlay";

    public bool isWalkable = true;
    public bool isSlimeable = true;
    public bool isTransparent = true;
    public Sprite groundSprite;
    public Sprite groundEffectSprite;
    public Sprite objectSprite;
    public Sprite overlaySprite;

    private SpriteRenderer _groundSpriteRenderer;
    private SpriteRenderer _groundEffectSpriteRenderer;
    private SpriteRenderer _objectSpriteRenderer;
    private SpriteRenderer _overlaySpriteRenderer;

    private const int MAX_POOL_SIZE = 8;
    private static Stack<HashSet<TileEntity>> _tileEntitySetPool = new Stack<HashSet<TileEntity>>();
    private HashSet<TileEntity> _containedTileEntities = null;

    public Vector2Int getTileSize() {
        return new Vector2Int(groundSprite.texture.width, groundSprite.texture.height) / TILE_PIXEL_SIZE;
    }

    public void OnDestroy() {
        if (_groundEffectSpriteRenderer) {
            Destroy(_groundEffectSpriteRenderer.gameObject);
        }
        if (_objectSpriteRenderer) {
            Destroy(_objectSpriteRenderer.gameObject);
        }
        if (_overlaySpriteRenderer) {
            Destroy(_overlaySpriteRenderer.gameObject);
        }
    }

    public void addTileEntity(TileEntity tileEntity) {
        if (_containedTileEntities == null) {
            if (_tileEntitySetPool.Count != 0) {
                _containedTileEntities = _tileEntitySetPool.Pop();
            } else {
                _containedTileEntities = new HashSet<TileEntity>();
            }
        }
        _containedTileEntities.Add(tileEntity);
    }

    public void removeTileEntity(TileEntity tileEntity) {
        _containedTileEntities.Remove(tileEntity);
        if (_containedTileEntities.Count == 0) {
            if (_tileEntitySetPool.Count != MAX_POOL_SIZE) {
                _tileEntitySetPool.Push(_containedTileEntities);
            }
            _containedTileEntities = null;
        }
    }

    public HashSet<TileEntity> getTileEntities() {
        return _containedTileEntities;
    }

    public bool canDamageEntities() {
        bool canDamage = false;
        if (_containedTileEntities != null) {
            foreach (TileEntity entity in _containedTileEntities) {
                IDamageable damageable = entity.GetComponent(typeof(IDamageable)) as IDamageable;
                if (damageable != null) {
                    canDamage = true;
                    break;
                }
            }
        }
        return canDamage;
    }
    /* Calling this method damages any TileEntities which are currently
     * standing on the Tile.  This method returns true only if 
     * at least one TileEntity was damaged.
     */
    public bool damageTileEntities(float damage) {
        bool didDamage = false;
        if (_containedTileEntities != null) {
            foreach (TileEntity entity in _containedTileEntities) {
                IDamageable damageable = entity.GetComponent(typeof(IDamageable)) as IDamageable;
                if (damageable != null) {
                    didDamage = true;
                    damageable.damage(damage);
                }
            }
        }
        return didDamage;
    }

    /* Calling this method stuns any TileEntities which are currently
     * standing on the Tile.  This method returns true only if 
     * at least one TileEntity was stunned.
     */
    public bool stunTileEntities(float duration) {
        bool wasStunned = false;
        if (_containedTileEntities != null) {
            foreach (TileEntity entity in _containedTileEntities) {
                IStunnable stunnableEntity = entity.GetComponent(typeof(IStunnable)) as IStunnable;
                if (stunnableEntity != null) {
                    wasStunned = true;
                    stunnableEntity.stun(duration);
                }
            }
        }
        return wasStunned;
    }

    /* This method causes the Tile object to update the sprite
     * renderers with the correct sprites, as well as (re)construct
     * the child game objects for the additional sprite overlays
     * 
     * This method is usually called by the tilemap editor so
     * that the prefabs are updated correctly in the editor
     * to remain visible
     */
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




    public static bool isWalkableFunction(Vector2Int location) {
        Tile tile = Tilemap.getInstance().getTile(location);
        if (tile == null) {
            return false;
        }

        return tile.isWalkable;
    }

    public static bool isSlimeableFunction(Vector2Int location) {
        Tile tile = Tilemap.getInstance().getTile(location);
        if (tile == null) {
            return false;
        }

        return tile.isSlimeable;
    }

    public static bool isTransparentFunction(Vector2Int location) {
        Tile tile = Tilemap.getInstance().getTile(location);
        if (tile == null) {
            return false;
        }

        return tile.isTransparent;
    }

    public static bool isSlimeFunction(Vector2Int location) {
        Tile tile = Tilemap.getInstance().getTile(location);
        if (tile == null) {
            return false;
        }

        return tile.GetComponent<Slime>() != null;
    }
}
