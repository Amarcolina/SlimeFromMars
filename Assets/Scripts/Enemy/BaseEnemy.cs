using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour {

	private float health;
	private float speed;
	private float damage;
	public float visionRange;

	private Vector2Int myTilePos;
	private Vector2Int playerTilePos;

	private Vector2 enemyPos;
	private Vector2 playerPos;

	public bool wander;
	public bool seek;

	public GameObject player;
	public GameObject enemy;

	private Tile _myTile;
	private Tilemap _tilemap;


	public Path wanderPath;
	private Path chasePath;
		
	// Load up tile map info on start for enemy movement
	public void Awake(){
		//Get enemy and player position in Vector2 space
		enemyPos = enemy.transform.position;
		playerPos = player.transform.position;

		//Convert the Vector2 positions of the gameobject into Vector2Int
		myTilePos = new Vector2Int ((int)enemyPos.x, (int)enemyPos.y);
		playerTilePos = new Vector2Int ((int)playerPos.x, (int)playerPos.y);

		_tilemap = Tilemap.getInstance();
		_myTile = GetComponent<Tile>();

		
		wanderPath = new Path ();
		wanderPath.addNodeToEnd (new Vector2Int (5, 0));
		wanderPath.addNodeToEnd (new Vector2Int (1, 0));
		wanderPath.addNodeToEnd (new Vector2Int (1, 1));
		wanderPath.addNodeToEnd (new Vector2Int (1, 2));
	}


	// gets the enemy chasing path
	public Path getChasePath(){
		chasePath = Astar.findPath(myTilePos, playerTilePos);
		return chasePath;
	}

	//Wander movement - follows the specified path
	public void Wander(){
		Vector2Int nextNode = wanderPath.getNext();
		Tile nextMoveTile = _tilemap.getTile(nextNode);
		Debug.Log ((int)nextNode.x + (int)nextNode.y);
		
		updatePosition(nextNode);
		//If the next node in the path is walkable, update the enemy position
		if (nextMoveTile && nextMoveTile.isWalkable) {
			Debug.Log("walkable");

		}
		}

	//Chase the player based on the path from A*
	public void Chase(){
		Vector2Int nextNode = chasePath.getNext();
		
		Tile nextMoveTile = _tilemap.getTile(nextNode);
	}

	//Updates the position when walking in a path
	public void updatePosition(Vector2Int nextNode){
		enemyPos = new Vector2((int)nextNode.x,(int)nextNode.y);
		enemy.transform.position = enemyPos;

	}

	//Update behavior
	void Update(){

		if (wanderPath != null) {
			Wander();	
			Debug.Log("wandering");
		}
		if (health <= 0) {
			GameObject.Destroy(this);		
		}


	}

}
