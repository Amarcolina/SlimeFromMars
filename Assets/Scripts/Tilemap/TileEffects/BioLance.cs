using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BioLance : MonoBehaviour {
    Path lancePath;
    float lanceDamage;
    public const float SPINE_SPEED = 10f;
    private SpineRenderer _spineRenderer;

    public AnimationCurve outLengthCurve;
    public AnimationCurve inLengthCurve;

    // Use this for initialization
    void Start() {
        _spineRenderer = GetComponent<SpineRenderer>();
        _spineRenderer.spinePath = lancePath;
        StartCoroutine(spineCoroutine());
    }

    private IEnumerator spineCoroutine() {
        for (float percent = 0; percent <= 1; percent += (SPINE_SPEED / (lancePath.getLength())) * Time.deltaTime) {
            _spineRenderer.spineLengthPercent = outLengthCurve.Evaluate(percent);//set to percent
            yield return null;//wait for one frame
        }

        Vector2Int tilePosition = lancePath.getEnd();
        Tile tile = Tilemap.getInstance().getTile(tilePosition);
        HashSet<TileEntity> tileEntities = tile.getTileEntities();

        GameObject interactionObject = null;

        if(tileEntities != null){
            foreach (TileEntity entity in tileEntities) {
                IDamageable damageable = entity.GetComponent(typeof(IDamageable)) as IDamageable;
                if (damageable != null) {
                    interactionObject = (entity.gameObject);
                    if (damageable is IGrabbable) {
                        break;
                    }
                }

                IGrabbable grabbable = entity.GetComponent(typeof(IGrabbable)) as IGrabbable;
                if (grabbable != null && interactionObject == null) {
                    interactionObject = entity.gameObject;
                }
            }
        }

        IDamageable objDamageable = interactionObject == null ? null : interactionObject.GetComponent(typeof(IDamageable)) as IDamageable;
        IGrabbable objGrabbable = interactionObject == null ? null : interactionObject.GetComponent(typeof(IGrabbable)) as IGrabbable;

        if (objDamageable != null) {
            objDamageable.damage(lanceDamage);
        }

        if (objGrabbable != null) {
            interactionObject.GetComponent<TileEntity>().pickUp();
        }

        for (float percent = 0; percent <= 1; percent += (SPINE_SPEED / (lancePath.getLength())) * Time.deltaTime) {
            _spineRenderer.spineLengthPercent = inLengthCurve.Evaluate(percent);//set to percent
            if (objGrabbable != null && interactionObject != null) {
                interactionObject.transform.position = _spineRenderer.getTip();
            }

            yield return null;//wait for one frame
        }

        if (objGrabbable != null) {
            interactionObject.GetComponent<TileEntity>().putDown();
        }

        if (interactionObject != null) {
            interactionObject.GetComponent<TileEntity>().forceUpdate();
        }

        Destroy(gameObject);
    }

    public void setLanceDamage(float damage) {
        lanceDamage = damage;
    }

    public void setLancePath(Path path) {
        lancePath = path;
    }
}
