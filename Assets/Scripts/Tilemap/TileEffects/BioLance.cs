using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BioLance : MonoBehaviour {
    Path lancePath;
    int lanceDamage;
    public const float SPINE_SPEED = 10;
    private SpineRenderer _spineRenderer;

    // Use this for initialization
    void Start() {
        _spineRenderer = GetComponent<SpineRenderer>();
        _spineRenderer.spinePath = lancePath;
        StartCoroutine(spineCoroutine());
    }

    private IEnumerator spineCoroutine() {
        for (float percent = 0; percent <= 1; percent += (SPINE_SPEED / (lancePath.getLength())) * Time.deltaTime) {
            _spineRenderer.spineLengthPercent = percent;//set to percent
            yield return null;//wait for one frame
        }

        Vector2Int tilePosition = lancePath.getEnd();
        Tile tile = Tilemap.getInstance().getTile(tilePosition);
        HashSet<TileEntity> tileEntities = tile.getTileEntities();

        GameObject damageableObject = null;
        IDamageable damageable = null;
        bool shouldGrab = false;

        if(tileEntities != null){
            foreach (TileEntity entity in tileEntities) {
                damageable = entity.GetComponent(typeof(IDamageable)) as IDamageable;
                if (damageable!= null) {
                    damageableObject = (entity.gameObject);
                    if (damageable is IGrabbable) {
                        shouldGrab = true;
                        break;
                    }
                }
            }
        }

        damageable.damage(lanceDamage);

        for (float percent = 1; percent >= 0; percent -= (SPINE_SPEED / (lancePath.getLength())) * Time.deltaTime) {
            _spineRenderer.spineLengthPercent = percent;//set to percent
            if (damageableObject != null && shouldGrab) {
                damageableObject.transform.position = lancePath.getSmoothPoint(percent*(lancePath.Count - 1));
            }

            yield return null;//wait for one frame
        }
        Destroy(gameObject);
    }
    public void setLanceDamage(int damage) {
        lanceDamage = damage;
    }

    public void setLancePath(Path path) {
        lancePath = path;
    }
}
