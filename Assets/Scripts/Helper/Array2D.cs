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

    /* Acts like a constructor, since ScriptableObjects cannot
     * have constructors.  Creates a new 2D array with a given
     * width and height.
     * 
     * @param width     The width of the array to create
     * @param height    The height of the array to create
     */ 
    public void init(int width, int height) {
        _array = new T[height * width];
        _width = width;
        _height = height;
    }

    public bool isInRange(Vector2Int index) {
        return index.x >= 0 && index.y >= 0 && index.x < width && index.y < height;
    }

    /* Gets the width of the array
     */
    public int width {
        get {
            return _width;
        }
    }

    /* Gets the height of the array
     */
    public int height {
        get {
            return _height;
        }
    }

    /* Overrides the [#,#] operator so that it can be accessed
     * like a normal 2D array.  This operator allows you to get
     * or set the element at the location x,y
     */
    public T this[int x, int y] {
        get {
            return _array[y * _width + x];
        }
        set {
            _array[y * _width + x] = value;
        }
    }
}
