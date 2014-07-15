using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureCombiner : MonoBehaviour {
    private static Dictionary<long, Sprite> _textureDictionary = new Dictionary<long, Sprite>();

    public static Sprite combineTextures(params Sprite[] sprites) {
        long hash = 0;
        foreach (Sprite s in sprites) {
            if (s) {
                hash += s.GetHashCode();
            }
        }

        if (_textureDictionary.ContainsKey(hash)) {
            return _textureDictionary[hash];
        }

        int length = 0;
        for (int i = 0; i < sprites.Length; i++) {
            if (sprites[i] != null) {
                sprites[length] = sprites[i];
                length++;
            }
        }

        for (int i = 1; i < length; i++) {
            if (sprites[i].texture.width != sprites[i - 1].texture.width || sprites[i].texture.height != sprites[i - 1].texture.height) {
                Debug.LogError("Cannot combine Sprites of different sizes!\nSprites " + sprites[i] + " and " + sprites[i - 1] + " have different sizes!");
                return null;
            }
        }

        Color[][] colors = new Color[length][];
        for (int i = 0; i < length; i++) {
            colors[i] = sprites[i].texture.GetPixels();
        }

        for (int colorIndex = 0; colorIndex < colors[0].Length; colorIndex++) {
            Color baseColor = colors[0][colorIndex];
            for (int textureIndex = 1; textureIndex < length; textureIndex++) {
                Color nextColor = colors[textureIndex][colorIndex];
                baseColor = baseColor * (1 - nextColor.a) + nextColor * nextColor.a;
            }
            colors[0][colorIndex] = baseColor;
        }

        Texture2D newTexture = new Texture2D(sprites[0].texture.width, sprites[0].texture.height, TextureFormat.ARGB32, false);
        newTexture.SetPixels(colors[0]);
        newTexture.filterMode = FilterMode.Point;
        newTexture.Apply();

        Sprite firstSprite = sprites[0];
        Sprite sprite = Sprite.Create(newTexture, new Rect(0, 0, firstSprite.texture.width, firstSprite.texture.height), new Vector2(0.5f, 0.5f), firstSprite.texture.width);
        sprite.name = "Combined Sprite";
        _textureDictionary[hash] = sprite;

        return sprite;
    }
}
