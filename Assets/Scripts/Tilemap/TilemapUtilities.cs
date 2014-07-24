using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct TileRayHit {
    public Vector2Int hitPosition;
    public Vector2Int previousPosition;
    public int tilesTraveled;
}


public class TilemapUtilities {
    //####################################################################################################
    /* Returns the neighboring position to a specific location in the tilemap.
     * The user can specify if they want to include the diagonal tiles in the 
     * neighbor list, as well as if they want to include non-walkable tiles in the list
     */
    public static List<Vector2Int> getNeighboringPositions(Vector2Int position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        List<Vector2Int> neighborList = new List<Vector2Int>();
        FoundNeighborFunc func = delegate(Vector2Int tilePosition) {
            neighborList.Add(tilePosition);
        };
        findNeighboringLocationsInternal(position, func, includeNonWalkable, includeDiagonal);
        return neighborList;
    }

    public static List<Vector2Int> getNeighboringPositions(Vector2 position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        return getNeighboringPositions(Tilemap.getTilemapLocation(position), includeNonWalkable, includeDiagonal);
    }

    public static List<Tile> getNeighboringTiles(Vector2Int position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        List<Tile> neighborList = new List<Tile>();
        FoundNeighborFunc func = delegate(Vector2Int tilePosition) {
            neighborList.Add(Tilemap.getInstance().getTile(tilePosition));
        };
        findNeighboringLocationsInternal(position, func, includeNonWalkable, includeDiagonal);
        return neighborList;
    }

    public static List<Tile> getNeighboringTiles(Vector2 position, bool includeNonWalkable = false, bool includeDiagonal = true) {
        return getNeighboringTiles(Tilemap.getTilemapLocation(position), includeNonWalkable, includeDiagonal);
    }

    private delegate void NeighborBuilderDelegate(Vector2Int delta);
    private delegate void FoundNeighborFunc(Vector2Int tilePosition);
    private static void findNeighboringLocationsInternal(Vector2Int position, FoundNeighborFunc func, bool includeNonWalkable = false, bool includeDiagonal = true) {
        Tilemap tilemap = Tilemap.getInstance();
        
        NeighborBuilderDelegate buildNeighborList = delegate(Vector2Int delta) {
            Tile tile = tilemap.getTile(position + delta);
            if (tile != null) {
                if (tile.isWalkable || includeNonWalkable) {
                    func(position + delta);
                }
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
    /* Returns the neighboring position to a specific location in the tilemap.
     * The user can specify if they want to include the diagonal tiles in the 
     * neighbor list, as well as if they want to include non-walkable tiles in the list
     */
    public delegate bool TileRayHitFunction(GameObject tileObject);
    public static bool defaultRayHitFunction(GameObject tileObject) {
        return tileObject == null || !tileObject.GetComponent<Tile>().isWalkable;
    }

    public static TileRayHit castTileRay(Vector2Int start, Vector2 direction, float maxDistance, TileRayHitFunction hitFunction = null) {
        return castTileRay(start, start + Tilemap.getTilemapLocation(direction.normalized * maxDistance), hitFunction);
    }

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
        }

        TileRayHit rayHit;
        rayHit.hitPosition = nextPosition;
        rayHit.previousPosition = currPosition;
        rayHit.tilesTraveled = 0;

        return rayHit;
    }
}
