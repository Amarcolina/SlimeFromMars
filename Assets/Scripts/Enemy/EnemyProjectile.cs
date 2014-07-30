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

        float fireAngle = Mathf.Atan2(direction.y, direction.y) * Mathf.Rad2Deg;
        float angleInsideOfCone = fireAngle + Random.Range(-30, 30);
        transform.eulerAngles = new Vector3(0, 0, angleInsideOfCone);
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
        //transform.position = transform.forward;

    }

    void FixedUpdate()
    {
        // Apply movement to the rigidbody\
        rigidbody2D.velocity = movement;
    }
}