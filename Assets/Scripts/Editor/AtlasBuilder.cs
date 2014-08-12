using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class AtlasBuilder {
    private Texture2D _atlasTexture = null;
    private int _offsetX = 0, _offsetY = 0;

    public AtlasBuilder(int width, int height, string path) {
        _atlasTexture = new Texture2D(width, height, TextureFormat.ARGB32, true, false);
        _atlasTexture.filterMode = FilterMode.Trilinear;
        AssetDatabase.CreateAsset(_atlasTexture, path);   
    }

    public void clear() {

    }

    public Sprite addSprite(Sprite sprite) {
        Texture2D sourceTex = sprite.texture;

        for(int x=0; x<68; x++){
            int texX = Mathf.Clamp(x-2, 0, 63) + (int)sprite.textureRect.x;
            for(int y=0; y<68; y++){
                int texY = Mathf.Clamp(y-2, 0, 63) + (int)sprite.textureRect.y;

                Color sourceColor = sourceTex.GetPixel(texX, texY);
                if(x == 0 || y == 0 || x == 67 || y == 67){
                    sourceColor.a = 0;
                }

                _atlasTexture.SetPixel(x + _offsetX, y + _offsetY, sourceColor);
            }
        }
        _atlasTexture.Apply();

        Sprite newSprite = Sprite.Create(_atlasTexture, new Rect(2 + _offsetX, 2 + _offsetY, 64, 64), new Vector2(0.5f, 0.5f));
        AssetDatabase.AddObjectToAsset(newSprite, _atlasTexture);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newSprite));

        _offsetX += 68;
        if (_offsetX + 68 >= _atlasTexture.width) {
            _offsetX = 0;
            _offsetY += 68;
        }

        return newSprite;
    }

    public void finalize() {
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_atlasTexture));
    }
}
