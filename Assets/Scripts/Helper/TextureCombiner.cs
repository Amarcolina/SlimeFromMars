using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureCombiner : MonoBehaviour {
    private static Dictionary<Texture2D[], Texture2D> _textureDictionary = new Dictionary<Texture2D[], Texture2D>();
    
    public static Texture2D combineTextures(Texture2D[] textures) {
        if (_textureDictionary.ContainsKey(textures)) {
            return _textureDictionary[textures];
        }

        for (int i = 1; i < textures.Length; i++) {
            if (textures[i].width != textures[i - 1].width || textures[i].height != textures[i - 1].height) {
                Debug.LogError("Cannot combine textures of different sizes!\nTextures " + textures[i] + " and " + textures[i - 1] + " have different sizes!");
                return null;
            }
        }

        Color[][] colors = new Color[textures.Length][];
        for (int i = 0; i < textures.Length; i++) {
            colors[i] = textures[i].GetPixels();
        }

        for (int i = 0; i < colors[0].Length; i++) {
            Color baseColor = colors[0][i];
            for (int j = 1; j < colors[0].Length; j++) {
                Color nextColor = colors[i][j];
                baseColor = baseColor * (1 - nextColor.a) + nextColor * nextColor.a;
            }
            colors[0][i] = baseColor;
        }

        Texture2D newTexture = new Texture2D(textures[0].width, textures[1].height);
        newTexture.SetPixels(colors[0]);
        _textureDictionary[textures] = newTexture;

        return newTexture;
    }
}
