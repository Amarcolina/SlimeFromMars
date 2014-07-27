using UnityEngine;
using System.Collections;

/* A non-generic extension of Array2D for Tiles
 * This is so that Unity can serialize the class
 * Since unity can not serialize generic classes
 */
[System.Serializable]
public class Array2DTile : Array2D<Tile> { }