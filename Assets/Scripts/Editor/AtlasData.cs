using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasData : ScriptableObject {
    public int offsetX = 0, offsetY = 0;
    public List<Sprite> atlasSprites = new List<Sprite>();
    public List<Sprite> originalSprites = new List<Sprite>();
}
