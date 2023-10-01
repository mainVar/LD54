using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomTextureImporter : AssetPostprocessor
{
    // This method is called before importing a texture
    private void OnPreprocessTexture()
    {
        if (assetPath.Contains("_SpriteAtlas"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;

            // Set the texture import settings as desired
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritePixelsPerUnit = 100; // Adjust as needed
            textureImporter.filterMode = FilterMode.Bilinear; // Adjust as needed
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed; // Adjust as needed

            // Specify the sprite slice size (128x128)
            int sliceSize = 128;
            textureImporter.spritesheet = new[]
            {
                new SpriteMetaData
                {
                    name = "Slice1", // You can customize the sprite names
                    rect = new Rect(0, 0, sliceSize, sliceSize),
                    pivot = new Vector2(0.5f, 0.5f), // Set pivot as desired
                    border = new Vector4(0, 0, 0, 0) // Set border as desired
                },
                new SpriteMetaData
                {
                    name = "Slice2",
                    rect = new Rect(sliceSize, 0, sliceSize, sliceSize),
                    pivot = new Vector2(0.5f, 0.5f),
                    border = new Vector4(0, 0, 0, 0)
                },
                // Add more SpriteMetaData entries for additional slices
            };
        }
    }
}
