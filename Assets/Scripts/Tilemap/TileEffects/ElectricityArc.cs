using UnityEngine;
using System.Collections;

public class ElectricityArc : MonoBehaviour {
    int arcRadius, arcDamage, arcNumber;
    private float timeUntilArc = 0.2f;

    void Start() {
        Tilemap.getInstance().getTile(transform.position).damageTileEntities(arcDamage);
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

    //creates a single arc 
    private void doArc() {
        for (int dx = -arcRadius; dx <= arcRadius; dx++) {
            for (int dy = -arcRadius; dy <= arcRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= arcRadius * arcRadius) {
                    Tile tile = Tilemap.getInstance().getTile(Tilemap.getTilemapLocation(transform.position) + new Vector2Int(dx, dy));
                    if (tile != null && tile.canDamageEntities() && arcNumber > 0) {
                        GameObject electricityArc = new GameObject("ElectricityArc");
                        electricityArc.transform.position = tile.transform.position;
                        ElectricityArc arcComponent = electricityArc.AddComponent<ElectricityArc>();
                        arcComponent.setArcDamage(arcDamage);
                        arcComponent.setArcNumber(arcNumber - 1);
                        //initializes arcRadius in new component for created game object
                        arcComponent.setArcRadius(arcRadius);
                        return;
                    }
                }
            }
        }
    }
}
