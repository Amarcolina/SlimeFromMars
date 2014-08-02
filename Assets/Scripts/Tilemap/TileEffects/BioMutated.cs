using UnityEngine;
using System.Collections;

public class BioMutated : MonoBehaviour {
    private static Sprite[] _bioSprites = null;
    private SpriteRenderer _bioRenderer = null;
    private const float SLIME_HEALTH = 5;
    private void initBioSprites() {
        _bioSprites = new Sprite[1];
        _bioSprites[0] = Resources.Load<Sprite>("Sprites/Elements/Bio/Bio00");
    }

    void Awake() {
        if (_bioSprites == null) {
            initBioSprites();
        }
        Tilemap.getInstance().getTile(transform.position).GetComponent<Slime>().upgradeHealth(SLIME_HEALTH);
        GameObject rendererGameObject = new GameObject("Bio");
        rendererGameObject.transform.parent = transform;
        rendererGameObject.transform.position = transform.position;
        _bioRenderer = rendererGameObject.AddComponent<SpriteRenderer>();
        _bioRenderer.sprite = _bioSprites[0];
    }

    public void OnDestroy() {
        Destroy(_bioRenderer.gameObject);
    }

	// Update is called once per frame
	void Update () {
	
	}
}
