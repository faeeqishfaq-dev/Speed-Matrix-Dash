using UnityEngine;
using UnityEditor;
using System.IO;

namespace GeometryDash.Editor
{
    public static class VFXAssetGenerator
    {
        //[InitializeOnLoadMethod]
        public static void GenerateSquareParticleAsset()
        {
            string folderPath = "Assets/Art/VFX";
            string filePath = folderPath + "/SquareParticle.png";

            if (!AssetDatabase.IsValidFolder("Assets/Art"))
                AssetDatabase.CreateFolder("Assets", "Art");
            
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Art", "VFX");

            if (!File.Exists(filePath))
            {
                // Create a 16x16 white texture
                Texture2D tex = new Texture2D(16, 16);
                Color[] colors = new Color[16 * 16];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                tex.SetPixels(colors);
                tex.Apply();

                // Save to disk
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
                
                AssetDatabase.Refresh();

                // Modify texture importer settings to make it a Sprite
                TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
                
                Debug.Log("[VFX] Successfully generated SquareParticle.png at " + filePath);
            }
        }
    }
}
