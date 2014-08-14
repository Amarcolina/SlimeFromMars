using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectricityArc : MonoBehaviour {
    float arcRadius;
    int arcNumber;
    float arcDamage;
    private float timeUntilArc = 0.2f;
    private Vector3 _destination;

    private static GameObject _electricArcPrefab = null;

    private AudioClip electricArcSFX;
    private SoundManager sound;

    private HashSet<TileEntity> _alreadyHitEntities = null;

    void Start() {
        if (_alreadyHitEntities == null) {
            _alreadyHitEntities = new HashSet<TileEntity>();
        }
        sound = SoundManager.getInstance();

        Tile destinationTile = Tilemap.getInstance().getTile(_destination);
        destinationTile.damageTileEntities(arcDamage);

        HashSet<TileEntity> _tileEntities = destinationTile.getTileEntities();
        if (_tileEntities != null) {
            foreach (TileEntity entity in _tileEntities) {
                _alreadyHitEntities.Add(entity);
            }
        }

        electricArcSFX = Resources.Load<AudioClip>("Sounds/SFX/electric_arc");

        if (_electricArcPrefab == null) {
            _electricArcPrefab = Resources.Load<GameObject>("Particles/ChainLightning");
        }

        float yAngle = -Mathf.Atan2(_destination.y - transform.position.y, _destination.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion arcAngle = Quaternion.Euler(yAngle, 90, 0);
        GameObject arcObject = Instantiate(_electricArcPrefab, transform.position + Vector3.back, arcAngle) as GameObject;
        arcObject.GetComponent<ParticleSystem>().startSize = Vector3.Distance(transform.position, _destination) / 7.0f;


        sound.PlaySound(arcObject.transform, electricArcSFX, true);
        Destroy(arcObject, 1.5f);

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

    public void setArcRadius(float radius) {
        arcRadius = radius;
    }

    public void setArcDamage(float damage) {
        arcDamage = damage;
    }

    public void setArcNumber(int number) {
        arcNumber = number;
    }

    public void setDestination(Vector3 destination) {
        _destination = destination;
    }

    public void setAlreadyHitSet(HashSet<TileEntity> set) {
        _alreadyHitEntities = set;
    }

    //creates a single arc 
    private void doArc() {
        int intRadius = (int)arcRadius + 1;

        Tile closestTile = null;
        float closestDistanceSqr = float.MaxValue;

        for (int dx = -intRadius; dx <= intRadius; dx++) {
            for (int dy = -intRadius; dy <= intRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                float sqrDistance = tileOffset.sqrMagnitude;
                if (sqrDistance > closestDistanceSqr) {
                    continue;
                }

                if (sqrDistance <= arcRadius * arcRadius) {
                    Vector2Int potentialJumpLocation = (Vector2Int)_destination + new Vector2Int(dx, dy);
                    Tile tile = Tilemap.getInstance().getTile(potentialJumpLocation);

                    if (tile != null && tile.canDamageEntities() && tile.gameObject.GetComponent<Electrified>() == null && arcNumber > 0) {

                        HashSet<TileEntity> _potentialEntities = tile.getTileEntities();
                        bool foundNewEntity = false;
                        foreach (TileEntity entity in _potentialEntities) {
                            if (entity.GetComponent(typeof(IDamageable)) != null) {
                                if (!_alreadyHitEntities.Contains(entity)) {
                                    foundNewEntity = true;
                                    break;
                                }
                            }
                        }

                        //If this tile doesn't hold an entity we havent jumped to yet, we cannot jump to it, so we stop
                        if (!foundNewEntity) {
                            continue;
                        }

                        //If we can't see the tile, we skip it.  This is way down here because it is expensive
                        TileRayHit hit = TilemapUtilities.castTileRay(_destination, potentialJumpLocation, null);
                        if(hit.didHit){
                            continue;
                        }

                        closestTile = tile;
                        closestDistanceSqr = sqrDistance;
                    }
                }
            }
        }

        if (closestTile != null) {
            GameObject electricityArc = new GameObject("ElectricityArc");
            electricityArc.transform.position = _destination;

            foreach (TileEntity entity in closestTile.getTileEntities()) {
                _alreadyHitEntities.Add(entity);
            }

            ElectricityArc arcComponent = electricityArc.AddComponent<ElectricityArc>();

            arcComponent.setArcDamage(arcDamage * 0.75f);
            arcComponent.setArcNumber(arcNumber - 1);
            arcComponent.setDestination(closestTile.transform.position);
            arcComponent.setAlreadyHitSet(_alreadyHitEntities);

            //initializes arcRadius in new component for created game object
            arcComponent.setArcRadius(arcRadius);
        }
    }
}
