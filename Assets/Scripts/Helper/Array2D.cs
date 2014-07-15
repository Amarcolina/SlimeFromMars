using UnityEngine;
using System.Collections;

[System.Serializable]
public class Array2D<T> : ScriptableObject where T : class{
    [SerializeField]
    private T[] _array;
    [SerializeField]
    private int _width;
    [SerializeField]
    private int _height;

    public void init(int width, int height) {
        _array = new T[height * width];
        _width = width;
        _height = height;
    }

    public int width {
        get {
            return _width;
        }
    }

    public int height {
        get {
            return _height;
        }
    }

    public T this[int x, int y] {
        get {
            return _array[y * _width + x];
        }
        set {
            _array[y * _width + x] = value;
        }
    }
}
