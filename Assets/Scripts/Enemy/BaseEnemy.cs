using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour
{

//private Vector3 currwaypoint;
//private Vector3 nextwaypoint;
//private Path chase;

		private const float  SPEED = 2.5f;
		private float curTime;

//How long the enemy stays idle once it reaches the waypoint
		public float pauseDuration;
		public bool hasLoop;
		public Transform[] waypoints;
		private int waypointcount;
		private Tile _myTile;
		private Tilemap _tilemap;


/*Simple enemy movement based on waypoints and run through a loop if wanted.
 *Currently ignores tiles until I get a better understanding
 *of how the tile system works :S
 *
 *For now, we will just have to manually place waypoints
 *for the enemies and position them in such a way that it looks
 *fine
 */
// Load up tile map info on start for enemy movement
		public void Awake ()
		{
				_tilemap = Tilemap.getInstance ();
				_myTile = GetComponent<Tile> ();
		}

		//Update behavior
		void Update ()
		{
				checkForSlimeTiles ();
				//Check to see if we go through the list of waypoints
				if (waypointcount < waypoints.Length) {
						followWayPoints ();		
				} else {
						//If we do loop, reset the waypoint counter
						if (hasLoop) {		
								waypointcount = 0;
						}
				}
		}

		//Checks to see if the enemytileobject has a slime component on its tile
		public void checkForSlimeTiles ()
		{
				GameObject enemyTileObj = _tilemap.getTileGameObject (transform.position);
				if (enemyTileObj.GetComponent<Slime> () != null) {
						Destroy (this.gameObject);
				}
		}
	
		public void followWayPoints ()
		{

				//Get the position of the waypoint in the list of waypoints
				Vector3 targetwaypoint = waypoints [waypointcount].position;

				//Get the distance of movement
				Vector3 distanceToWayPoint = targetwaypoint - transform.position;

				//If we land very close onto a waypoint
				if (distanceToWayPoint.magnitude < 0.01) {
						//count up to the next waypoint
						if (curTime == 0)
								curTime = Time.time; // Pause over the Waypoint
						if ((Time.time - curTime) >= pauseDuration) {
								waypointcount++;
								curTime = 0;
						}
				} else {
						//move the enemy to the next waypoint
						this.gameObject.transform.position = Vector3.MoveTowards (this.gameObject.transform.position, targetwaypoint, SPEED * Time.deltaTime);
				}
		}

/*These methods got commented out because I will still need to implement them in the upcoming days
 * Mostly junk because I still dont have a good understanding of the tile system.
	
//Wander movement - follows the specified path
public void Wander (Path path)
{
updatePosition (path);
		
}
	
//Chase the player based on the path from A*
public void Chase (Path path)
{
		
//Convert the Vector2 positions of the gameobject into Vector2Int
Vector2Int startLocation = Tilemap.getTilemapLocation (enemy.transform.position);
Vector2Int goalLocation = Tilemap.getTilemapLocation (player.transform.position);
		
//This path is null here, not sure why
chase = Astar.findPath (startLocation, goalLocation);
if (chase == null) {
Debug.Log("Empty path");		
	}
}

	
//Updates the position when walking in a path
public void updatePosition (Path path)
{
Vector3 enemyPos = enemy.transform.position;
//Vector3 playerPos = player.transform.position;
		
//check to see if we reached waypoint
Vector3 offset = currwaypoint - enemyPos;
float d = Vector3.SqrMagnitude (offset);
if (d < 0.0001f) {
//set next way point with the next node
nextwaypoint = new Vector3 ((float)path.getNext ().x, (float)path.getNext ().y, 0);
currwaypoint = nextwaypoint;
}

//move player according to the next node in the path
enemy.transform.position = Vector3.MoveTowards (enemy.transform.position, currwaypoint, speed * Time.deltaTime);
} */
}