using UnityEngine;
using System.Collections;

[System.Serializable]
public class Vector2Int {
    public int x, y;

    /* Creates a default Vector where X and Y are
     * both initialized to zero
     */
    public Vector2Int() {
        this.x = 0;
        this.y = 0;
    }

    /* Creates a new Vector2Int and sets the initial
     * values of both x and y
     */
    public Vector2Int(int x, int y) {
        this.x = x;
        this.y = y;
    }

    /* Overrides the + operator so that this class can be
     * added to itself
     */
    public static Vector2Int operator +(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    /* Overrides the - operator so that this class can be
     * subtracted from itself
     */
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }

    /* Overrides the * operator so that this class can be scaled
     * by a value
     */
    public static Vector2Int operator *(Vector2Int a, int x) {
        return new Vector2Int(a.x * x, a.y * x);
    }

    /* Overrides the / operator so that this class can be divided
     * by a value
     */
    public static Vector2Int operator /(Vector2Int a, int d) {
        return new Vector2Int(a.x / d, a.y / d);
    }

    /* Returns the distance between 2 Vector2Ints
     */
    public static float distance(Vector2Int a, Vector2Int b) {
        return (a - b).getLength();
    }

    /* Returns the length of this Vector2Int
     */
    public float getLength() {
        return Mathf.Sqrt(x * x + y * y);
    }

    public int getLengthSqrd() {
        return x * x + y * y;
    }

    /* Overrides the == operator so that this class can be compared
     * to itself
     */
    public static bool operator ==(Vector2Int a, Vector2Int b) {
        //If they are litteraly the same reference, always return true
        if (System.Object.ReferenceEquals(a, b)) {
            return true;
        }

        //If one object is null but not the other, return false
        if (((object)a == null) != ((object)b == null)) {
            return false;
        }

        //Return true if they are the same value
        return a.x == b.x && a.y == b.y;
    }

    /* Overrides the != operator so that this class can be compared
     * against itself
     */
    public static bool operator !=(Vector2Int a, Vector2Int b) {
        return !(a == b);
    }

    public override bool Equals(object obj) {
        if (!(obj is Vector2Int)) {
            return false;
        }
        return (Vector2Int)obj == this;
    }

    public override int GetHashCode() {
        return x*(1 << 8) + y;
    }

    public static implicit operator Vector2(Vector2Int i) {
        return Tilemap.getWorldLocation(i);
    }

    public static implicit operator Vector3(Vector2Int i) {
        return Tilemap.getWorldLocation(i);
    }

    public static implicit operator Vector2Int(Vector2 i) {
        return Tilemap.getTilemapLocation(i);
    }

    public static implicit operator Vector2Int(Vector3 i) {
        return Tilemap.getTilemapLocation(i);
    }

    public static Vector2Int right {
        get {
            return new Vector2Int(1, 0);
        }
    }

    public static Vector2Int left {
        get {
            return new Vector2Int(-1, 0);
        }
    }

    public static Vector2Int up {
        get {
            return new Vector2Int(0, 1);
        }
    }

    public static Vector2Int down {
        get {
            return new Vector2Int(0, -1);
        }
    }

    public static Vector2Int zero {
        get {
            return new Vector2Int(0, 0);
        }
    }


}
