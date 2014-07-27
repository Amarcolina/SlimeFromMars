using UnityEngine;

/// <summary>
/// Simply moves the projectiles
/// </summary>
public class EnemyProjectile : BaseEnemy
{
    /// <summary>
    /// Object speed
    /// </summary>
    public Vector2 speed;

    /// <summary>
    /// Moving direction
    /// </summary>
    private Vector2 direction;

    /// <summary>
    /// Moving direction
    /// </summary>
    public float life;

    private Vector2 movement;

    private Slime _nearestSlime = null;
    private Slime damagedSlime;


    void Start()
    {
        if (transform != null)
        {
            _nearestSlime = getNearestVisibleSlime();

            direction = _nearestSlime.transform.position - transform.position;
            // 2 - Semi-Random movement
            movement = new Vector2(
             speed.x * Random.Range(direction.x - 1f, direction.x + 1f),
              speed.y * Random.Range(direction.y - 1f, direction.y + 1f));
        }

        Destroy(gameObject, life);
    }

    void Update()
    {
        GameObject tileGameObject = _tilemap.getTileGameObject(transform.position);
        if (tileGameObject != null)
        {
            //Check if we are on a slime tile
            if (tileGameObject.GetComponent<Slime>() != null)
            {
                //Get instance of the Slime tile
                Slime s = Tilemap.getInstance().getTileGameObject(transform.position).GetComponent<Slime>();
                s.damageSlime(5f);
            }
        }
    }

    void FixedUpdate()
    {
        // Apply movement to the rigidbody
        rigidbody2D.velocity = movement;
    }
}