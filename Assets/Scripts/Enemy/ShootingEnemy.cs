using UnityEngine;
using System.Collections;


public class ShootingEnemy : MonoBehaviour {
    public const float FIRE_COOLDOWN = 0.15f;

    public int shootingRange = 10;
    public int shootingWidth = 5;
    public bool facingRight = false;

    private EnemyAnimation _enemyAnimation;
    private Tilemap _tilemap;
    private float _shotCooldown = 0.0f;

    private float getDirectionFloat() {
        return facingRight ? 1.0f : -1.0f;
    }

    public void Awake() {
        _enemyAnimation = GetComponent<EnemyAnimation>();
        _enemyAnimation.Flip(getDirectionFloat());
        _tilemap = Tilemap.getInstance();
    }

    public void Update() {
        if (_shotCooldown > 0.0f) {
            _shotCooldown -= Time.deltaTime;
            _enemyAnimation.EnemyShoot(getDirectionFloat());
        } else {
            scanVision();
        }
    }

    private void scanVision() {
        for (float i = 0; i <= shootingRange; i++) {
            for (float j = -shootingWidth / 2.0f + 0.5f; j <= shootingWidth / 2.0f - 0.5f; j++) {
                Vector2 checkPosition = transform.position + new Vector3(i * getDirectionFloat(), j, 0);
                GameObject tileGameObject = _tilemap.getTileGameObject(checkPosition);
                if (tileGameObject) {
                    Slime slime = tileGameObject.GetComponent<Slime>();
                    if (slime != null  &&  slime.isSolid()) {
                        slime.damageSlime(0.9f);
                        _shotCooldown = FIRE_COOLDOWN;
                        return;
                    }
                }
            }
        }
    }


    public void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
        Vector3 cubeExtents = new Vector3(shootingRange, shootingWidth, 1);
        Vector3 cubeCenter = transform.position + new Vector3(shootingRange / 2.0f, 0, 0) * getDirectionFloat();
        Gizmos.DrawCube(cubeCenter, cubeExtents);
    }
}
