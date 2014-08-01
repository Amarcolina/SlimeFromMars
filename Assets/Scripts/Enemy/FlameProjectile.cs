using UnityEngine;

/// <summary>
/// Simply moves the projectiles
/// </summary>
public class FlameProjectile : MonoBehaviour
{
    /// <summary>
    /// projectile speed
    /// </summary>
    public float speed;

    /// <summary>
    /// Moving direction
    /// </summary>
    public Vector2 direction;

    /// <summary>
    /// projectile life
    /// </summary>
    public float life;

    private GameObject tileGameObject;
    private Slime slime;

    void Start()
    {
        //Not sure what I'm doing here
        float fireAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angleInsideOfCone = fireAngle + Random.Range(-20, 20);
        transform.eulerAngles = new Vector3(0, 0, angleInsideOfCone);

        //Destroy after finished its life
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
                //damage the slime
                slime.damageSlime(0.25f);
            }
        }
        //Move projectile along its rotation
        transform.position += transform.right * Time.deltaTime * speed;
    }
}