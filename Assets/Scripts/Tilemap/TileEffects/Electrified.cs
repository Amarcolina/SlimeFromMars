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
        _electricityRenderer = rendererGameObject.AddComponent<SpriteRenderer>();
	}

    public void Update() {
        Sprite randomSprite = _electricitySprites[Random.Range(0, _electricitySprites.Length)];
        _electricityRenderer.sprite = randomSprite;
        _electricityRenderer.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 3) * 90.0f);

        HashSet<TileEntity> entities = _tile.getTileEntities();
        foreach (TileEntity entity in entities) {
            IDamageable damageable = entity.GetComponent(typeof (IDamageable)) as IDamageable;
            if (damageable != null) {
                damageable.damage(TOTAL_DAMAGE * Time.deltaTime / DURATION);
            }
        }

        _totalTime += Time.deltaTime;
        if (_totalTime >= DURATION) {
            Destroy(this);
        }
    }
}
