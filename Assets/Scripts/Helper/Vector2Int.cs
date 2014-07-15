using UnityEngine;
using System.Collections;

[System.Serializable]
public class Vector2Int {
    [SerializeField]
    private int _x;
    [SerializeField]
    private int _y;

    public int x {
        get {
            return _x;
        }
    }

    public int y {
        get {
            return _y;
        }
    }

    public Vector2Int(int x, int y) {
        _x = x;
        _y = y;
    }

    public static Vector2Int operator +(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    public static Vector2Int operator -(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }

    public static Vector2Int operator *(Vector2Int a, int x) {
        return new Vector2Int(a.x * x, a.y * x);
    }

    public static Vector2Int operator /(Vector2Int a, int d) {
        return new Vector2Int(a.x / d, a.y / d);
    }
}
