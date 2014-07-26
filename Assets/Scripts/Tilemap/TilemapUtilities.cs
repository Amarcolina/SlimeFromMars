using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* This struct is returned by any of the Tile Ray Cast methods
 */
public struct TileRayHit {
    // Hit Position represents the location where the ray hit a Tile
    public Vector2Int hitPosition;
    // Previous Position represents the last "safe" tile before the ray hit
    public Vector2Int previousPosition;
    // Tiles Traveled represents the total number of tiles checked before the hit
    public int tilesTraveled;
    // DidHit returns whether or not the ray hit something, or got to the end of the ray
    public bool didHit;
}


public class TilemapUtilities {
    //####################################################################################################
    /* Returns the neighboring position to a specific location in the tilemap.
     * The user can specify if they want to include the diagonal tiles in the 
     * neighbor list, as well as if they want to include non-walkable tiles in the list
     */

    public static Vector2Int[] neighborOrthoArray = {new Vector2Int(0, 1),
                                                     new Vector2Int(1, 0),
                                                     new Vector2Int(0, -1),
                                                     new Vector2Int(-1, 0)};

    public static Vector2Int[] neighborFullArray = {new Vector2Int(-1, -1),
                                                    new Vector2Int( 0, -1),
                                                    new Vector2Int( 1, -1),
                                                    new Vector2Int( 1,  0),
                                                    new Vector2Int( 1,  1),
                                                    new Vector2Int( 0,  1),
                                                    new Vector2Int(-1,  1),
                                                    new Vector2Int(-1,  0)};

    public static bool areTilesNeighbors(Vector2Int tile0, Vector2Int tile1, bool checkDiagonal = true, AStarIsPathWalkable walkableFunction = null) {
        if (walkableFunction == null) {
            walkableFunction = Astar.defaultIsWalkable;
        }

        if (!walkableFunction(tile1)) {
            return false;
        }

        if (tile0.x - tile1.x == 0 || tile0.y - tile1.y == 0) {
            return true;
        }

        if (!checkDiagonal) {
            return false;
        }

        bool cornerA = walkableFunction(new Vector2Int(tile0.x, tile1.y));
        bool cornerB = walkableFunction(new Vector2Int(tile1.x, tile0.y));
        return cornerA && cornerB;
    }

    //####################################################################################################
    /* Casts a ray across the tilemap along a given direction, for a specified distance.  By default, the
     * raycast will hit anything that is not walkable, but the user is able specify their own TileRayHitFunction
     * that specifies when the Ray hits something.  This can be used to look for specific types of objects
     */
    public static TileRayHit castTileRay(Vector2Int start, Vector2 direction, float maxDistance = 20, TileRayHitFunction hitFunction = null) {
        return castTileRay(start, start + Tilemap.getTilemapLocation(direction.normalized * maxDistance), hitFunction);
    }

    /* Casts a ray just like the above function, but given a destination end of the ray instead of a 
     * direction.  This can be used to search and detect whether or not a certain tile is visible
     * from another.
     */
    public static TileRayHit castTileRay(Vector2Int start, Vector2Int end, TileRayHitFunction hitFunction = null) {
        if (hitFunction == null) {
            hitFunction = defaultRayHitFunction;
        }

        Tilemap tilemap = Tilemap.getInstance();

        Vector2Int direction = end - start;
        int xAxis = direction.x > 0 ? 1 : -1;
        int yAxis = direction.y > 0 ? 1 : -1;

        Vector2Int rayAxis, altAxis;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            rayAxis = new Vector2Int(xAxis, 0);
            altAxis = new Vector2Int(0, yAxis);
        } else {
            rayAxis = new Vector2Int(0, yAxis);
            altAxis = new Vector2Int(xAxis, 0);
        }

        int residualDelta = Mathf.Min(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
        int residualCap = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
        int residual = residualCap / 2;

        TileRayHit rayHit;
        rayHit.tilesTraveled = 0;
        Vector2Int currPosition = start;
        Vector2Int nextPosition = start;

        while (!hitFunction(tilemap.getTileGameObject(nextPosition)) && nextPosition != end) {
            currPosition = nextPosition;

            nextPosition += rayAxis;

            residual += residualDelta;
            if (residual >= residualCap) {
                residual -= residualCap;
                nextPosition += altAxis;
            }

            rayHit.tilesTraveled++;
        }

        
        rayHit.hitPosition = nextPosition;
        rayHit.previousPosition = currPosition;
        rayHit.didHit = nextPosition != end;

        return rayHit;
    }

    public delegate bool TileRayHitFunction(GameObject tileObject);
    public static bool defaultRayHitFunction(GameObject tileObject) {
        return tileObject == null || !tileObject.GetComponent<Tile>().isWalkable;
    }
}
