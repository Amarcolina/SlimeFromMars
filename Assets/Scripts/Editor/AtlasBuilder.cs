using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AtlasBuilder {
    private AtlasData _atlasData = null;
    private Texture2D _atlasTexture = null;
    private string _path;

    public AtlasBuilder(string path) {
        _path = path;
        _atlasData = AssetDatabase.LoadAssetAtPath(path, typeof(AtlasData)) as AtlasData;
        if (_atlasData == null) {
            Debug.Log("No atlas found, creating asset");
            clear();
        } else {
            _atlasTexture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(_atlasData), typeof(Texture2D)) as Texture2D;
        }
    }

    public void clear() {
        _atlasData = ScriptableObject.CreateInstance<AtlasData>();
        AssetDatabase.CreateAsset(_atlasData, _path);
        AssetDatabase.SaveAssets();
    }

    public void startNewAtlas(int width, int height) {
        clear();

        _atlasTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _atlasTexture.filterMode = FilterMode.Trilinear;
        _atlasData.offsetX = 0;
        _atlasData.offsetY = 0;
        AssetDatabase.AddObjectToAsset(_atlasTexture, _atlasData);
    }

    public bool hasAtlas() {
        return _atlasTexture != null;
    }

    public Sprite getOriginalSprite(Sprite atlasSprite) {
        for (int i = 0; i < _atlasData.atlasSprites.Count; i++) {
            if (atlasSprite == _atlasData.atlasSprites[i]) {
                return _atlasData.originalSprites[i];
            }
        }
        return null;
    }

    public int getSpriteCount() {
        if (_atlasData == null) {
            return 0;
        }
        return _atlasData.originalSprites.Count;
    }

    public Sprite addSprite(Sprite sourceSprite) {
        Texture2D sourceTex = sourceSprite.texture;

        for (int i = 0; i < _atlasData.originalSprites.Count; i++) {
            if (_atlasData.originalSprites[i] == sourceSprite) {
                return _atlasData.atlasSprites[i];
            }
        }

        for (int x = 0; x < 68; x++) {
            int texX = Mathf.Clamp(x - 2, 0, 63) + (int)sourceSprite.textureRect.x;
            for (int y = 0; y < 68; y++) {
                int texY = Mathf.Clamp(y - 2, 0, 63) + (int)sourceSprite.textureRect.y;

                Color sourceColor = sourceTex.GetPixel(texX, texY);
                if (x == 0 || y == 0 || x == 67 || y == 67) {
                    sourceColor.a = 0;
                }

                _atlasTexture.SetPixel(x + _atlasData.offsetX, y + _atlasData.offsetY, sourceColor);
            }
        }
        _atlasTexture.Apply();

        Sprite newSprite = Sprite.Create(_atlasTexture, new Rect(2 + _atlasData.offsetX, 2 + _atlasData.offsetY, 64, 64), new Vector2(0.5f, 0.5f), 64);
        newSprite.name = "[Atlas]" + sourceSprite.name;

        AssetDatabase.AddObjectToAsset(newSprite, _atlasTexture);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newSprite));

        _atlasData.offsetX += 68;
        if (_atlasData.offsetX + 68 >= _atlasTexture.width) {
            _atlasData.offsetX = 0;
            _atlasData.offsetY += 68;
        }

        _atlasData.atlasSprites.Add(newSprite);
        _atlasData.originalSprites.Add(sourceSprite);

        return newSprite;
    }

    public void finalize() {
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_atlasTexture));
    }
}
