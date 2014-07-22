using UnityEngine;
using System.Collections;

[System.Serializable]
public class Array2D : ScriptableObject{
    [SerializeField]
    private Object[] _array;
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
    public static Array2D createArray(int width, int height) {
        Array2D instance = ScriptableObject.CreateInstance<Array2D>();
        instance._array = new Object[height * width];
        instance._width = width;
        instance._height = height;
        return instance;
    }

    /* Returns whether or not the given 2D index falls inside
     * of this 2D array.  
     */
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
    public Object this[int x, int y] {
        get {
            return _array[y * _width + x];
        }
        set {
            _array[y * _width + x] = value;
        }
    }

    public override string ToString(){
        return "Array2D: (" + width + " , " + height + ")";
    }
}
