using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Electrified : MonoBehaviour {
    public const float DURATION = 1.0f;
    public const float TOTAL_DAMAGE = 0.1f;

    private static Sprite[] _electricitySprites = null;
    private SpriteRenderer _electricityRenderer = null;
    private float _totalTime = 0.0f;
    private Tile _tile;

    /* Sprites are static so they can be shared between all 
     * Electried components.  This method is called by the 
     * first Electrified component to initialize the needed
     * resources
     */
    private void initElectricSprites() {
        _electricitySprites = new Sprite[1];
        _electricitySprites[0] = Resources.Load<Sprite>("Sprites/Elements/Electricity/Electricity00");
    }

	void Awake () {
        if (_electricitySprites == null) {
            initElectricSprites();
        }

        _tile = GetComponent<Tile>();

        GameObject rendererGameObject = new GameObject("Electricity");
        rendererGameObject.transform.parent = transform;
        rendererGameObject.transform.position = transform.position;
        _electricityRenderer = rendererGameObject.AddComponent<SpriteRenderer>();
	}

    public void OnDestroy() {
        Destroy(_electricityRenderer.gameObject);
    }

    /* This update will run for DURATION amount of time.  It will damage any 
     * tile enties that exist on the tile that have an IDamageable component
     * connected to their game object.  It will damage the objects a little bit
     * each frame
     */
    public void Update() {
        Sprite randomSprite = _electricitySprites[Random.Range(0, _electricitySprites.Length)];
        _electricityRenderer.sprite = randomSprite;
        _electricityRenderer.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90.0f);

        HashSet<TileEntity> entities = _tile.getTileEntities();
        if (entities != null) {
            foreach (TileEntity entity in entities) {
                IDamageable damageable = entity.GetComponent(typeof(IDamageable)) as IDamageable;
                if (damageable != null) {
                    damageable.damage(TOTAL_DAMAGE * Time.deltaTime / DURATION);
                }
            }
        }
        

        _totalTime += Time.deltaTime;
        if (_totalTime >= DURATION) {
            Destroy(this);
        }
    }
}
