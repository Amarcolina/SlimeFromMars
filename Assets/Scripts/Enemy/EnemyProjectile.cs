using UnityEngine;

/// <summary>
/// Simply moves the projectiles
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    /// <summary>
    /// Object speed
    /// </summary>
    public float speed;

    /// <summary>
    /// Moving direction
    /// </summary>
    public Vector2 direction;

    /// <summary>
    /// Moving direction
    /// </summary>
    public float life;

    private GameObject tileGameObject;
    private Slime slime;
    private Vector2 movement;

    void Start()
    {
        if (transform != null)
        {
            // 2 - Semi-Random movement
            movement = speed * direction;
        }
        //Get instance of the Slime tile


        Destroy(gameObject, life);
    }

    void Update()
    {
        tileGameObject = Tilemap.getInstance().getTileGameObject(transform.position);

        if (tileGameObject != null)
        {

            slime = tileGameObject.GetComponent<Slime>();
            //Check if we are on a slime tile
            if (slime != null)
            {
                //Get instance of the Slime tile
                slime.damageSlime(5f);
            }
        }

    }

    void FixedUpdate()
    {
        // Apply movement to the rigidbody
        rigidbody2D.velocity = movement;
    }
}