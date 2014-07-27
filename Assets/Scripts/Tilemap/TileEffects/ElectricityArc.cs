using UnityEngine;
using System.Collections;

public class ElectricityArc : MonoBehaviour {
    int arcRadius, arcDamage, arcNumber;
    private float timeUntilArc = 0.2f;
    private Vector3 _destination;

    private static GameObject _electricArcPrefab = null;

    void Start() {
        if (_electricArcPrefab == null) {
            _electricArcPrefab = Resources.Load<GameObject>("Particles/ChainLightning");
        }

        float yAngle = -Mathf.Atan2(_destination.y - transform.position.y, _destination.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion arcAngle = Quaternion.Euler(yAngle, 90, 0);
        GameObject arcObject = Instantiate(_electricArcPrefab, transform.position + Vector3.back, arcAngle) as GameObject;
        arcObject.GetComponent<ParticleSystem>().startSize = Vector3.Distance(transform.position, _destination) / 7.0f;
        Destroy(arcObject, 1.0f);

        Tilemap.getInstance().getTileGameObject(_destination).AddComponent<Electrified>();
    }
    // Update is called once per frame
    void Update() {
        if (timeUntilArc > 0) {
            timeUntilArc -= Time.deltaTime;
        } else {
            if (arcNumber > 0) {
                doArc();
            }
            Destroy(this.gameObject);
        }
    }

    public void setArcRadius(int radius) {
        arcRadius = radius;
    }

    public void setArcDamage(int damage) {
        arcDamage = damage;
    }

    public void setArcNumber(int number) {
        arcNumber = number;
    }

    public void setDestination(Vector3 destination) {
        _destination = destination;
    }

    //creates a single arc 
    private void doArc() {
        for (int dx = -arcRadius; dx <= arcRadius; dx++) {
            for (int dy = -arcRadius; dy <= arcRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= arcRadius * arcRadius) {
                    Tile tile = Tilemap.getInstance().getTile(Tilemap.getTilemapLocation(_destination) + new Vector2Int(dx, dy));
                    if (tile != null && tile.canDamageEntities() && tile.gameObject.GetComponent<Electrified>() == null && arcNumber > 0) {
                        GameObject electricityArc = new GameObject("ElectricityArc");
                        electricityArc.transform.position = _destination;
                        ElectricityArc arcComponent = electricityArc.AddComponent<ElectricityArc>();
                        arcComponent.setArcDamage(arcDamage);
                        arcComponent.setArcNumber(arcNumber - 1);
                        arcComponent.setDestination(tile.transform.position);
                        //initializes arcRadius in new component for created game object
                        arcComponent.setArcRadius(arcRadius);
                        return;
                    }
                }
            }
        }
    }
}
