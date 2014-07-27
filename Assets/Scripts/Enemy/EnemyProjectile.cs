using UnityEngine;
using System.Collections;

public class EnemyProjectile : BaseEnemy
{
    private float lifeSpan = 2.0f;
    public Transform pExplosion;
    public Transform soundExplosion;
    private float projectileSpeed = 10f;
    public Transform player;
    Vector3 dir;


    private Slime _nearestSlime = null;



    void Update()
    {
        _nearestSlime = getNearestVisibleSlime();
        player = _nearestSlime.transform;

        dir = player.position - transform.position;

        rigidbody.velocity = dir.normalized * projectileSpeed;
        //Destroys the projectile after it has lived its life
        Destroy(this.gameObject, lifeSpan);

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag != "Enemy")
        {
            if (other.gameObject.tag != "EnemyProjectile")
            {
                soundExplosion.audio.Play();
                Instantiate(pExplosion, transform.position, transform.rotation);
                Destroy(this.gameObject);

            }
        }
    }

}