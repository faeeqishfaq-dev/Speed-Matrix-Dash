using UnityEngine;
using UnityEditor;
using System.IO;

namespace GeometryDash.Editor
{
    public static class PhaseTwoArtGenerator
    {
        private static string artPath = "Assets/Art/PhaseTwo";

        [MenuItem("Tools/Geometry Dash/1. Generate Phase 2 Art & Prefabs")]
        public static void GenerateArtAndPrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
                AssetDatabase.CreateFolder("Assets", "Art");
            
            if (!AssetDatabase.IsValidFolder("Assets/Art/PhaseTwo"))
                AssetDatabase.CreateFolder("Assets/Art", "PhaseTwo");
            
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Environment"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Prefabs", "Environment");
            }

            GenerateTextures();
            AssetDatabase.Refresh();
            
            // Allow refresh to finish before creating prefabs
            EditorApplication.delayCall += CreatePrefabs;
        }

        private static void GenerateTextures()
        {
            // 1. Brick Texture (Dark, Tileable)
            Texture2D brickTex = new Texture2D(64, 64);
            Color dark = new Color(0.1f, 0.1f, 0.15f);
            Color shadow = new Color(0.05f, 0.05f, 0.08f);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    bool isMortar = (y % 16 == 0) || (y % 32 < 16 && x % 32 == 0) || (y % 32 >= 16 && (x + 16) % 32 == 0);
                    brickTex.SetPixel(x, y, isMortar ? shadow : dark);
                }
            }
            brickTex.Apply();
            SaveTexture(brickTex, "PlatformBrick.png");

            // 2. Neon Edge (Green)
            Texture2D neonGreen = new Texture2D(64, 16);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distFromTop = 15 - y;
                    float glow = Mathf.Clamp01(1f - (distFromTop / 8f));
                    neonGreen.SetPixel(x, y, new Color(0.2f * glow, 1f * glow, 0.2f * glow, 1f));
                }
            }
            neonGreen.Apply();
            SaveTexture(neonGreen, "NeonEdge_Green.png");

            // 3. Neon Edge (Purple)
            Texture2D neonPurple = new Texture2D(64, 16);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distFromTop = 15 - y;
                    float glow = Mathf.Clamp01(1f - (distFromTop / 8f));
                    neonPurple.SetPixel(x, y, new Color(0.8f * glow, 0.2f * glow, 1f * glow, 1f));
                }
            }
            neonPurple.Apply();
            SaveTexture(neonPurple, "NeonEdge_Purple.png");

            // 4. Spike Texture
            Texture2D spikeTex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    // Triangle shape
                    float widthAtY = 64f - y;
                    float distFromCenter = Mathf.Abs(x - 32);
                    if (distFromCenter < widthAtY / 2f)
                    {
                        // Outline vs inner
                        bool isOutline = (distFromCenter > (widthAtY / 2f) - 4) || (y < 4);
                        spikeTex.SetPixel(x, y, isOutline ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f));
                    }
                    else
                    {
                        spikeTex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            spikeTex.Apply();
            SaveTexture(spikeTex, "Spike.png");

            // 5. Saw Texture
            Texture2D sawTex = new Texture2D(64, 64);
            Vector2 center = new Vector2(32, 32);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < 30f)
                    {
                        // Add some teeth (sin wave around angle)
                        float angle = Mathf.Atan2(y - 32, x - 32);
                        float wave = Mathf.Sin(angle * 12f) * 2f;
                        if (dist < 28f + wave)
                        {
                            bool isOutline = dist > 24f + wave;
                            sawTex.SetPixel(x, y, isOutline ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f));
                        }
                        else
                        {
                            sawTex.SetPixel(x, y, Color.clear);
                        }
                    }
                    else
                    {
                        sawTex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            sawTex.Apply();
            SaveTexture(sawTex, "Saw.png");

            // 6. Toxic Liquid Texture
            Texture2D toxicTex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (y > 56)
                    {
                        toxicTex.SetPixel(x, y, new Color(0.5f, 1f, 0.2f, 0.8f)); // Bright green top
                    }
                    else
                    {
                        float val = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                        toxicTex.SetPixel(x, y, new Color(0.1f, 0.5f + val * 0.3f, 0.1f, 0.8f)); // Bubbling liquid
                    }
                }
            }
            toxicTex.Apply();
            SaveTexture(toxicTex, "ToxicLiquid.png");
        }

        private static void SaveTexture(Texture2D tex, string fileName)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(artPath, fileName), bytes);
        }

        private static void CreatePrefabs()
        {
            // Set texture importers
            ConfigureTexture("PlatformBrick.png", TextureWrapMode.Repeat);
            ConfigureTexture("NeonEdge_Green.png", TextureWrapMode.Clamp);
            ConfigureTexture("NeonEdge_Purple.png", TextureWrapMode.Clamp);
            ConfigureTexture("Spike.png", TextureWrapMode.Clamp);
            ConfigureTexture("Saw.png", TextureWrapMode.Clamp);
            ConfigureTexture("ToxicLiquid.png", TextureWrapMode.Repeat);

            // Layer setup
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1) { Debug.LogError("Ground layer missing!"); groundLayer = 8; }
            int hazardLayer = LayerMask.NameToLayer("Hazard");
            if (hazardLayer == -1) { Debug.LogError("Hazard layer missing!"); hazardLayer = 9; }

            // 1. Create Platform_Green
            CreatePlatformPrefab("Platform_Green", "PlatformBrick.png", "NeonEdge_Green.png", groundLayer);
            
            // 2. Create Platform_Purple
            CreatePlatformPrefab("Platform_Purple", "PlatformBrick.png", "NeonEdge_Purple.png", groundLayer);

            // 3. Create Spike
            CreateHazardPrefab("Hazard_Spike", "Spike.png", hazardLayer, true);

            // 4. Create Saw (needs rotating script, we will just add a generic rotate later or leave it static)
            CreateHazardPrefab("Hazard_Saw", "Saw.png", hazardLayer, false);

            // 5. Create Toxic Liquid
            CreateToxicLiquidPrefab("Hazard_ToxicLiquid", "ToxicLiquid.png", hazardLayer);

            // 6. Level End Trigger
            CreateLevelEndPrefab();

            Debug.Log("[Phase 2] Art and Prefabs generated successfully!");
        }

        private static void ConfigureTexture(string fileName, TextureWrapMode wrapMode)
        {
            string path = $"{artPath}/{fileName}";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64;
                importer.filterMode = FilterMode.Point; // Crisp arcade look
                importer.wrapMode = wrapMode;
                if (fileName == "PlatformBrick.png" || fileName == "ToxicLiquid.png")
                {
                    // For 9-slicing or tiling, we just use standard sprite tiling in SpriteRenderer
                }
                importer.SaveAndReimport();
            }
        }

        private static void CreatePlatformPrefab(string name, string bgTex, string topTex, int layer)
        {
            GameObject root = new GameObject(name);
            root.layer = layer;

            SpriteRenderer bgSR = root.AddComponent<SpriteRenderer>();
            bgSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artPath}/{bgTex}");
            bgSR.drawMode = SpriteDrawMode.Tiled;
            bgSR.size = new Vector2(1, 1);
            bgSR.sortingOrder = 0;

            GameObject edge = new GameObject("TopEdge");
            edge.transform.SetParent(root.transform);
            edge.transform.localPosition = new Vector3(0, 0.375f, 0); // (1 - 0.25) / 2
            SpriteRenderer edgeSR = edge.AddComponent<SpriteRenderer>();
            edgeSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artPath}/{topTex}");
            edgeSR.drawMode = SpriteDrawMode.Tiled;
            edgeSR.size = new Vector2(1, 0.25f);
            edgeSR.sortingOrder = 1;

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1, 1);

            PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Environment/{name}.prefab");
            Object.DestroyImmediate(root);
        }

        private static void CreateHazardPrefab(string name, string tex, int layer, bool isSpike)
        {
            GameObject root = new GameObject(name);
            root.layer = layer;
            root.tag = "Hazard";

            SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artPath}/{tex}");
            sr.sortingOrder = 5;

            if (isSpike)
            {
                PolygonCollider2D col = root.AddComponent<PolygonCollider2D>();
                // Triangle
                col.points = new Vector2[] { new Vector2(-0.4f, -0.5f), new Vector2(0.4f, -0.5f), new Vector2(0, 0.4f) };
            }
            else
            {
                CircleCollider2D col = root.AddComponent<CircleCollider2D>();
                col.radius = 0.4f;
            }

            PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Environment/{name}.prefab");
            Object.DestroyImmediate(root);
        }

        private static void CreateToxicLiquidPrefab(string name, string tex, int layer)
        {
            GameObject root = new GameObject(name);
            root.layer = layer;
            root.tag = "Hazard";

            SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{artPath}/{tex}");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(1, 1);
            sr.sortingOrder = 4; // behind platforms

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1, 0.8f); // slightly lower hit box
            col.offset = new Vector2(0, -0.1f);
            col.isTrigger = true; // Use trigger for liquid usually, but wait, PlayerDeathController supports normal collision too.
            // PlayerDeathController triggers Die() on OnTriggerEnter2D if tag is Hazard. So trigger is fine!

            PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Environment/{name}.prefab");
            Object.DestroyImmediate(root);
        }

        private static void CreateLevelEndPrefab()
        {
            GameObject root = new GameObject("LevelEndTrigger");
            
            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1, 20); // tall trigger
            col.isTrigger = true;

            root.AddComponent<GeometryDash.Level.LevelEndTrigger>();

            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Environment/LevelEndTrigger.prefab");
            Object.DestroyImmediate(root);
        }
    }
}
