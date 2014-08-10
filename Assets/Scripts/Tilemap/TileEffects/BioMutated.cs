using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BioMutated : MonoBehaviour, ISaveable {
    private static Sprite[] _bioSprites = null;
    private SpriteRenderer _bioRenderer = null;
    private const float SLIME_HEALTH = 5;
    private void initBioSprites() {
        _bioSprites = new Sprite[1];
        _bioSprites[0] = Resources.Load<Sprite>("Sprites/Elements/Bio/Bio00");
    }

    void Start() {
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

    public void wither() {
        Destroy(this);
    }

    public void OnDestroy() {
        Destroy(_bioRenderer.gameObject);
    }

    public void onSave(Queue<object> data) { }
    public void onLoad(Queue<object> data) { }
}
