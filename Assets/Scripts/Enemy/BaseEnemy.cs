using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	private float health;
	private float speed;
	private float damage;
	public float visionRange;
	private Vector2Int position;
	public bool wander;
	public bool seek;
	public GameObject player;

	public void Spawn(Vector2Int spawnPosition){
		position = spawnPosition;
	}

	public void Wander(){
		}

	public void Chase(){
	}

	public void Update(){
		if (health <= 0) {
			GameObject.Destroy(this);		
		}
	}

}
