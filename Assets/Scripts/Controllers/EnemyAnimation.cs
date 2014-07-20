using UnityEngine;
using System.Collections;

// A class used to hold animation necessities for the enemies. Each Enemy should call these functions with a directional float indicating 
// their intended direction. 1 indicates facing right, -1 indicates facing left. The animator is the animatior that the prefab this script
// is attached to uses. It may be beneficial in the future to add this to the enemy movement decision code for simplicity.
public class EnemyAnimation : MonoBehaviour {

	public Animator enemy;
	public const string WALK_KEY = "walk";
	public const string SHOOT_KEY = "bulletfire";
	public const string FLAMETHROWER_KEY = "flamethrower";
	public const string HIT_KEY = "hit";



	//Called when enemy is ready to shoot flamethrower, requires direction input.
	public void EnemyFlameThrower(float direction){
		Flip (direction);
		enemy.SetBool(WALK_KEY, false);
		enemy.SetTrigger(FLAMETHROWER_KEY);
	}

	//Called when the enemy is ready to fire, requires a direction input.
	public void EnemyShoot(float direction){
		Flip (direction);
		enemy.SetBool(WALK_KEY, false);
		enemy.SetTrigger(SHOOT_KEY);
	}

	//Used for when an enemy is hit by the slime, or is absorbed by the slime
	public void EnemyHit(float direction){
		Flip(direction);
		enemy.SetBool(WALK_KEY, false);
		enemy.SetTrigger(HIT_KEY);
	}

	//Takes a directional float in order to flip the sprite in the correct direction. -1 for left facing, 1 for right facing.
	private void Flip(float direction){
		Vector3 absolutescale = new Vector3(Mathf.Abs(gameObject.transform.localScale.x), transform.localScale.y, transform.localScale.z);
		gameObject.transform.localScale = new Vector3(absolutescale.x * direction, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
	}

	//Called when the enemy needs to move, will play the enemy walking animation. Requires a directional float.
	public void EnemyMoving(float direction){
		Flip (direction);
		enemy.SetBool(WALK_KEY, true);
	}

	// An update loop designed simply to test features of this script. Remove this when functionality is ready to be combined.
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.W)){
			EnemyMoving(1);
		}

		if(Input.GetKeyDown(KeyCode.E)){
			//EnemyHit(-1);
			EnemyFlameThrower(-1);
		}

	}
}
