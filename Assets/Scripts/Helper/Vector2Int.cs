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

    /* Overrides the == operator so that this class can be compared
     * to itself
     */
    public static bool operator ==(Vector2Int a, Vector2Int b) {
        return a.x == b.x && a.y == b.y;
    }

    /* Overrides the != operator so that this class can be compared
     * against itself
     */
    public static bool operator !=(Vector2Int a, Vector2Int b) {
        return a.x != b.x || a.y != b.y;
    }
}
