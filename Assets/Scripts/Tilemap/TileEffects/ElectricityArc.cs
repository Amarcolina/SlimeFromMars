using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectricityArc : MonoBehaviour {
    int arcRadius, arcNumber;
    float arcDamage;
    private float timeUntilArc = 0.2f;
    private Vector3 _destination;

    private static GameObject _electricArcPrefab = null;

    private AudioClip electricArcSFX;

    private HashSet<TileEntity> _alreadyHitEntities = null;

    void Start() {
        if (_alreadyHitEntities == null) {
            _alreadyHitEntities = new HashSet<TileEntity>();
        }

        HashSet<TileEntity> _tileEntities = Tilemap.getInstance().getTile(_destination).getTileEntities();
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

        arcObject.AddComponent<SoundEffect>().sfx = electricArcSFX;
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

    public void setArcRadius(int radius) {
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

        List<Tile> _newJumpableTiles = new List<Tile>();
        List<Tile> _oldJumpableTiles = new List<Tile>();

        for (int dx = -arcRadius; dx <= arcRadius; dx++) {
            for (int dy = -arcRadius; dy <= arcRadius; dy++) {
                Vector2 tileOffset = new Vector2(dx, dy);
                if (tileOffset.sqrMagnitude <= arcRadius * arcRadius) {
                    Vector2Int potentialJumpLocation = (Vector2Int)_destination + new Vector2Int(dx, dy);
                    Tile tile = Tilemap.getInstance().getTile(potentialJumpLocation);

                    if (tile != null && tile.canDamageEntities() && tile.gameObject.GetComponent<Electrified>() == null && arcNumber > 0) {

                        HashSet<TileEntity> _potentialEntities = tile.getTileEntities();
                        bool foundNewEntity = false;
                        foreach (TileEntity entity in _potentialEntities) {
                            if (!_alreadyHitEntities.Contains(entity)) {
                                foundNewEntity = true;
                                break;
                            }
                        }

                        TileRayHit hit = TilemapUtilities.castTileRay(_destination, potentialJumpLocation, null);
                        if(hit.didHit){
                            continue;
                        }

                        if (foundNewEntity) {
                            _newJumpableTiles.Add(tile);
                        }
                        _oldJumpableTiles.Add(tile);
                    }
                }
            }
        }

        if (_newJumpableTiles.Count == 0) {
            _newJumpableTiles = _oldJumpableTiles;
        }

        if (_newJumpableTiles.Count != 0) {
            Tile jumpTile = _newJumpableTiles[Random.Range(0, _newJumpableTiles.Count)];
            GameObject electricityArc = new GameObject("ElectricityArc");
            electricityArc.transform.position = _destination;

            jumpTile.damageTileEntities(arcDamage);

            foreach (TileEntity entity in jumpTile.getTileEntities()) {
                _alreadyHitEntities.Add(entity);
            }

            ElectricityArc arcComponent = electricityArc.AddComponent<ElectricityArc>();
            arcComponent.setArcDamage(arcDamage);
            arcComponent.setArcNumber(arcNumber - 1);
            arcComponent.setDestination(jumpTile.transform.position);
            arcComponent.setAlreadyHitSet(_alreadyHitEntities);
            //initializes arcRadius in new component for created game object
            arcComponent.setArcRadius(arcRadius);
        }
    }
}
