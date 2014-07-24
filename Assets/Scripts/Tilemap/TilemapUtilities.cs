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

    
    public static List<Vector2Int> getNeighboringPositions(Vector2Int position, 
                                                           bool includeDiagonal = true,  
                                                           AStarIsPathWalkable walkableFunction = null) {
        List<Vector2Int> neighborList = new List<Vector2Int>();
        FoundNeighborFunc func = delegate(Vector2Int tilePosition) {
            neighborList.Add(tilePosition);
        };
        findNeighboringLocationsInternal(position, func, includeDiagonal, walkableFunction);
        return neighborList;
    }

    public static List<Tile> getNeighboringTiles(Vector2Int position, 
                                                 bool includeDiagonal = true, 
                                                 AStarIsPathWalkable walkableFunction = null) {
        List<Tile> neighborList = new List<Tile>();
        FoundNeighborFunc func = delegate(Vector2Int tilePosition) {
            neighborList.Add(Tilemap.getInstance().getTile(tilePosition));
        };
        findNeighboringLocationsInternal(position, func, includeDiagonal, walkableFunction);
        return neighborList;
    }

    private delegate void NeighborBuilderDelegate(Vector2Int delta);
    private delegate void FoundNeighborFunc(Vector2Int tilePosition);
    private static void findNeighboringLocationsInternal(Vector2Int position, FoundNeighborFunc func, bool includeDiagonal = true, AStarIsPathWalkable walkableFunction = null) {
        Tilemap tilemap = Tilemap.getInstance();
        
        NeighborBuilderDelegate buildNeighborList = delegate(Vector2Int delta) {
            Tile tile = tilemap.getTile(position + delta);
            if (tile != null) {
                func(position + delta);
            }
        };

        if (includeDiagonal) {
            if (tilemap.isWalkable(position + Vector2Int.right)) {
                if (tilemap.isWalkable(position + Vector2Int.up)) {
                    buildNeighborList(Vector2Int.right + Vector2Int.up);
                }
                if (tilemap.isWalkable(position + Vector2Int.down)) {
                    buildNeighborList(Vector2Int.right + Vector2Int.down);
                }
            }
            if (tilemap.isWalkable(position + Vector2Int.left)) {
                if (tilemap.isWalkable(position + Vector2Int.up)) {
                    buildNeighborList(Vector2Int.left + Vector2Int.up);
                }
                if (tilemap.isWalkable(position + Vector2Int.down)) {
                    buildNeighborList(Vector2Int.left + Vector2Int.down);
                }
            }
        }
        buildNeighborList(Vector2Int.right);
        buildNeighborList(Vector2Int.left);
        buildNeighborList(Vector2Int.up);
        buildNeighborList(Vector2Int.down);
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
