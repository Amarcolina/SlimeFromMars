using UnityEngine;
using System.Collections;
 
/* A non-generic extension of Array2D for TileChunks
 * This is so that Unity can serialize the class
 * Since unity can not serialize generi classes
 */
[System.Serializable]
public class Array2DTC : Array2D<TileChunk> { }
