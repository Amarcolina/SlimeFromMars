using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AtlasData : ScriptableObject {
    public List<Sprite> atlasSprites = new List<Sprite>();
    public List<Sprite> originalSprites = new List<Sprite>();
}

public class AtlasBuilder {
    private AtlasData _atlasData = null;
    private Texture2D _atlasTexture = null;
    private int _offsetX = 0, _offsetY = 0;

    public AtlasBuilder(string path) {
        _atlasData = AssetDatabase.LoadAssetAtPath(path, typeof(AtlasData)) as AtlasData;
        if (_atlasData == null) {
            Debug.Log("No atlas found, creating asset");
            _atlasData = ScriptableObject.CreateInstance<AtlasData>();
            AssetDatabase.CreateAsset(_atlasData, path);   
        } 
    }

    public void startNewAtlas(int width, int height) {
        _atlasData.originalSprites.Clear();
        _atlasData.atlasSprites.Clear();
        _atlasTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _atlasTexture.filterMode = FilterMode.Trilinear;
        _offsetX = 0;
        _offsetY = 0;
        AssetDatabase.AddObjectToAsset(_atlasTexture, _atlasData);
    }

    public Sprite getOriginalSprite(Sprite atlasSprite) {
        for (int i = 0; i < _atlasData.atlasSprites.Count; i++) {
            if (atlasSprite == _atlasData.atlasSprites[i]) {
                return _atlasData.originalSprites[i];
            }
        }
        return null;
    }

    public int spriteCount() {
        return _atlasData.originalSprites.Count;
    }

    public void clear() {
        foreach (Sprite sprite in _atlasData.atlasSprites) {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprite));
        }
        _atlasData.atlasSprites.Clear();
        _atlasData.originalSprites.Clear();
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_atlasTexture));
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

        Sprite newSprite = Sprite.Create(_atlasTexture, new Rect(2 + _offsetX, 2 + _offsetY, 64, 64), new Vector2(0.5f, 0.5f), 64);
        newSprite.name = "[Atlas]" + sprite.name;

        AssetDatabase.AddObjectToAsset(newSprite, _atlasTexture);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newSprite));

        _offsetX += 68;
        if (_offsetX + 68 >= _atlasTexture.width) {
            _offsetX = 0;
            _offsetY += 68;
        }

        _atlasData.atlasSprites.Add(newSprite);
        _atlasData.originalSprites.Add(sprite);

        return newSprite;
    }

    public void finalize() {
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_atlasTexture));
    }
}
