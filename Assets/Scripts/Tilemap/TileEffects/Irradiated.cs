using UnityEngine;
using System.Collections;

public class Irradiated : MonoBehaviour {
    public const float DURATION = 1.0f;
    public const float TOTAL_DAMAGE = 0.1f;
    private bool shouldStun;
    private bool shouldDamage;
    private static Sprite[] _radiationSprites = null;
    private SpriteRenderer _radiationRenderer = null;
    private float _totalTime = 0.0f;
    private Tile _tile;

    /* Sprites are static so they can be shared between all 
     * Irradiated components.  This method is called by the 
     * first Irradiated component to initialize the needed
     * resources
     */
    private void initRadiationSprites() {
        _radiationSprites = new Sprite[1];
        _radiationSprites[0] = Resources.Load<Sprite>("Sprites/Elements/Radiation/Radiation00");
    }

    void Awake() {
        if (_radiationSprites == null) {
            initRadiationSprites();
        }

        _tile = GetComponent<Tile>();

        GameObject rendererGameObject = new GameObject("Radiation");
        rendererGameObject.transform.parent = transform;
        rendererGameObject.transform.position = transform.position;
        _radiationRenderer = rendererGameObject.AddComponent<SpriteRenderer>();
    }

    public void setStunned(bool stun) {
        shouldStun = stun;
    }

    public void setDamaged(bool damage) {
        shouldDamage = damage;
    }
    public void OnDestroy() {
        Destroy(_radiationRenderer.gameObject);
    }

    /* This update will run for DURATION amount of time.  It will damage any 
     * tile enties that exist on the tile that have an IDamageable component
     * connected to their game object.  It will damage the objects a little bit
     * each frame
     */
    public void Update() {
        Sprite randomSprite = _radiationSprites[Random.Range(0, _radiationSprites.Length)];
        _radiationRenderer.sprite = randomSprite;
        _radiationRenderer.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90.0f);
        if (shouldDamage) {
            _tile.damageTileEntities(TOTAL_DAMAGE * Time.deltaTime / DURATION);

            _totalTime += Time.deltaTime;
            if (_totalTime >= DURATION) {
                Destroy(this);
            }
        }
        if (shouldStun) {
            _tile.stunTileEntities(DURATION);
        }
    }
}
