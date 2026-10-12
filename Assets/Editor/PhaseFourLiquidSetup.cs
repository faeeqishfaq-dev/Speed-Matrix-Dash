using UnityEngine;
using UnityEditor;
using GeometryDash.Level;
using System.IO;

namespace GeometryDash.Editor
{
    public static class PhaseFourLiquidSetup
    {
        [MenuItem("Tools/Geometry Dash/4. Run Phase 4 Liquid Setup")]
        public static void RunLiquidSetup()
        {
            GenerateTextures();
            AssetDatabase.Refresh();

            CreateMaterialsAndPrefabs();
            AssetDatabase.Refresh();

            InjectLiquidsIntoLevels();
            AssetDatabase.Refresh();

            Debug.Log("[Phase 4] Liquid Setup Complete!");
        }

        private static void GenerateTextures()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art/VFX"))
                AssetDatabase.CreateFolder("Assets/Art", "VFX");

            // 1. Noise Texture
            string noisePath = "Assets/Art/VFX/NoiseTexture.png";
            if (!File.Exists(noisePath))
            {
                int size = 128;
                Texture2D noise = new Texture2D(size, size);
                float scale = 4f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        // Seamless Perlin Noise approx
                        float px = (float)x / size;
                        float py = (float)y / size;
                        float n = Mathf.PerlinNoise(px * scale, py * scale);
                        noise.SetPixel(x, y, new Color(n, n, n, 1));
                    }
                }
                noise.Apply();
                File.WriteAllBytes(noisePath, noise.EncodeToPNG());
                AssetDatabase.ImportAsset(noisePath, ImportAssetOptions.ForceUpdate);
                
                TextureImporter imp = AssetImporter.GetAtPath(noisePath) as TextureImporter;
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.mipmapEnabled = true;
                imp.SaveAndReimport();
            }

            // 2. Liquid Bubble
            string bubblePath = "Assets/Art/VFX/LiquidBubble.png";
            if (!File.Exists(bubblePath))
            {
                Texture2D bubble = new Texture2D(32, 32);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x,y), new Vector2(16,16));
                        Color c = Color.clear;
                        if (d < 16 && d > 12) c = new Color(1, 1, 1, 0.8f);
                        else if (d <= 12) c = new Color(1, 1, 1, 0.3f);
                        bubble.SetPixel(x,y, c);
                    }
                }
                bubble.Apply();
                File.WriteAllBytes(bubblePath, bubble.EncodeToPNG());
                AssetDatabase.ImportAsset(bubblePath, ImportAssetOptions.ForceUpdate);
                
                TextureImporter imp = AssetImporter.GetAtPath(bubblePath) as TextureImporter;
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true;
                imp.SaveAndReimport();
            }

            // 3. Liquid Base White Tiled Texture
            string basePath = "Assets/Art/VFX/LiquidBase.png";
            if (!File.Exists(basePath))
            {
                Texture2D baseTex = new Texture2D(64, 64);
                for (int i=0; i<64*64; i++) baseTex.SetPixel(i%64, i/64, Color.white);
                baseTex.Apply();
                File.WriteAllBytes(basePath, baseTex.EncodeToPNG());
                AssetDatabase.ImportAsset(basePath, ImportAssetOptions.ForceUpdate);
                
                TextureImporter imp = AssetImporter.GetAtPath(basePath) as TextureImporter;
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.SaveAndReimport();
            }
        }

        private static void CreateMaterialsAndPrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials"))
                AssetDatabase.CreateFolder("Assets/Art", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/VFX"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "VFX");

            Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/VFX/NoiseTexture.png");

            // Lava Material
            Material lavaMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/LavaMaterial.mat");
            if (lavaMat == null)
            {
                lavaMat = new Material(Shader.Find("GeometryDash/LavaLiquid"));
                lavaMat.SetTexture("_NoiseTex", noiseTex);
                AssetDatabase.CreateAsset(lavaMat, "Assets/Art/Materials/LavaMaterial.mat");
            }

            // Water Material
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WaterMaterial.mat");
            if (waterMat == null)
            {
                waterMat = new Material(Shader.Find("GeometryDash/WaterLiquid"));
                waterMat.SetTexture("_NoiseTex", noiseTex);
                AssetDatabase.CreateAsset(waterMat, "Assets/Art/Materials/WaterMaterial.mat");
            }

            // Bubble Particle Prefab
            string prefabPath = "Assets/Prefabs/VFX/BubbleParticles.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                GameObject obj = new GameObject("BubbleParticles");
                ParticleSystem ps = obj.AddComponent<ParticleSystem>();
                
                var main = ps.main;
                main.duration = 5f;
                main.loop = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                var emission = ps.emission;
                emission.rateOverTime = 8f;
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Box; // Scale set by controller
                
                var size = ps.sizeOverLifetime;
                size.enabled = true;
                size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0,1,1,0));
                
                var renderer = obj.GetComponent<ParticleSystemRenderer>();
                Material pMat = new Material(Shader.Find("Sprites/Default"));
                pMat.mainTexture = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/LiquidBubble.png").texture;
                renderer.material = pMat;

                PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
                Object.DestroyImmediate(obj);
            }
        }

        private static void InjectLiquidsIntoLevels()
        {
            Material lavaMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/LavaMaterial.mat");
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/WaterMaterial.mat");
            GameObject bubblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/BubbleParticles.prefab");
            Sprite baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/LiquidBase.png");

            int updatedCount = 0;
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Transform fdzTransform = instance.transform.Find("FallDeathZone");
                if (fdzTransform != null)
                {
                    BoxCollider2D fdzCol = fdzTransform.GetComponent<BoxCollider2D>();
                    if (fdzCol != null)
                    {
                        // Clean up old
                        Transform oldLiquid = fdzTransform.Find("LiquidVisual");
                        if (oldLiquid != null) Object.DestroyImmediate(oldLiquid.gameObject);

                        float width = fdzCol.size.x;
                        float height = 12f;

                        // Create LiquidVisual
                        GameObject liquidObj = new GameObject("LiquidVisual");
                        liquidObj.transform.SetParent(fdzTransform);
                        
                        liquidObj.transform.localPosition = new Vector3(0, -4.75f, 0);

                        GameObject spriteObj = new GameObject("SpriteVisual");
                        spriteObj.transform.SetParent(liquidObj.transform, false);
                        SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
                        sr.sprite = baseSprite;
                        sr.sortingOrder = 10; 

                        LevelLiquidController llc = liquidObj.AddComponent<LevelLiquidController>();
                        llc.liquidType = (i % 2 != 0) ? LiquidType.Lava : LiquidType.Water;
                        llc.liquidWidth = width;
                        llc.liquidHeight = height;
                        
                        llc.GetType().GetField("lavaMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(llc, lavaMat);
                        llc.GetType().GetField("waterMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(llc, waterMat);
                        llc.GetType().GetField("bubbleParticlePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(llc, bubblePrefab);

                        llc.ApplySettings();
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                updatedCount++;
            }

            Debug.Log($"[Phase 4] Liquid injected into {updatedCount} levels.");
        }
    }
}
