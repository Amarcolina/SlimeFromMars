using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureCombiner : MonoBehaviour {
    private static Dictionary<Sprite[], Sprite> _textureDictionary = new Dictionary<Sprite[], Sprite>();

    public static Sprite combineTextures(params Sprite[] sprites) {
        if (_textureDictionary.ContainsKey(sprites)) {
            return _textureDictionary[sprites];
        }

        Sprite[] orderedSprites = new Sprite[sprites.Length];
        int length = 0;
        for (int i = 1; i < sprites.Length; i++) {
            if (sprites[i] != null) {
                orderedSprites[length] = sprites[i];
                length++;
            }
        }

        for (int i = 0; i < length; i++) {
            if (orderedSprites[i].texture.width != orderedSprites[i - 1].texture.width || orderedSprites[i].texture.height != orderedSprites[i - 1].texture.height) {
                Debug.LogError("Cannot combine Sprites of different sizes!\nSprites " + orderedSprites[i] + " and " + orderedSprites[i - 1] + " have different sizes!");
                return null;
            }
        }

        Color[][] colors = new Color[length][];
        for (int i = 0; i < length; i++) {
            colors[i] = orderedSprites[i].texture.GetPixels();
        }

        for (int i = 0; i < length; i++) {
            Color baseColor = colors[0][i];
            for (int j = 1; j < colors[0].Length; j++) {
                Color nextColor = colors[i][j];
                baseColor = baseColor * (1 - nextColor.a) + nextColor * nextColor.a;
            }
            colors[0][i] = baseColor;
        }

        Texture2D newTexture = new Texture2D(orderedSprites[0].texture.width, orderedSprites[0].texture.height);
        newTexture.SetPixels(colors[0]);

        Sprite sprite = Sprite.Create(newTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        _textureDictionary[sprites] = sprite;

        return sprite;
    }
}
