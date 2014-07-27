using UnityEngine;

/// <summary>
/// Simply moves the current game object
/// </summary>
public class EnemyProjectile : BaseEnemy
{
    // 1 - Designer variables

    /// <summary>
    /// Object speed
    /// </summary>
    public Vector2 speed = new Vector2(1, 1);

    /// <summary>
    /// Moving direction
    /// </summary>
    //public Vector2 direction = new Vector2(-1, 0);
    private Vector2 direction;

    private Vector2 movement;

    private Slime _nearestSlime = null;
    private Slime damagedSlime;


    void Start()
    {        
                if(transform!=null){
        _nearestSlime = getNearestVisibleSlime();

        direction = _nearestSlime.transform.position - transform.position;
        // 2 - Movement
        movement = new Vector2(
         1f * Random.Range(direction.x -1f, direction.x  + 1f),
          1f* Random.Range(direction.y -1f, direction.y  + 1f));
    }
    }

    void Update()
    {
        GameObject tileGameObject = _tilemap.getTileGameObject(transform.position);
        if (tileGameObject != null)
        {
            if (tileGameObject.GetComponent<Slime>() != null)
            {
                Slime s = Tilemap.getInstance().getTileGameObject(transform.position).GetComponent<Slime>();
                s.damageSlime(5f);
            }
        }
        Destroy(this, 2f);
    }

    void FixedUpdate()
    {
        // Apply movement to the rigidbody
        rigidbody2D.velocity = movement;
    }
}