using UnityEngine;
using UnityEditor;
using GeometryDash.Level;
using System.Collections.Generic;

namespace GeometryDash.Editor
{
    public static class PhaseTwoLevelGenerator
    {
        [MenuItem("Tools/Geometry Dash/2. Generate Phase 2 Levels")]
        public static void GenerateLevels()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Levels"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Levels");
            }

            for (int i = 1; i <= 15; i++)
            {
                GenerateLevel(i);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase 2] All 15 Levels generated successfully!");
        }

        private static void GenerateLevel(int levelIndex)
        {
            GameObject levelRoot = new GameObject($"Level_{levelIndex:00}");
            LevelDefinition def = levelRoot.AddComponent<LevelDefinition>();
            def.LevelIndex = levelIndex;

            // Anchors
            GameObject startAnchor = new GameObject("StartAnchor");
            startAnchor.transform.SetParent(levelRoot.transform);
            startAnchor.transform.localPosition = Vector3.zero;
            def.StartAnchor = startAnchor.transform;

            GameObject endAnchor = new GameObject("EndAnchor");
            endAnchor.transform.SetParent(levelRoot.transform);
            def.EndAnchor = endAnchor.transform;

            GameObject baselineAnchor = new GameObject("GameplayBaselineAnchor");
            baselineAnchor.transform.SetParent(levelRoot.transform);
            baselineAnchor.transform.localPosition = Vector3.zero; // Starts at 0
            def.GameplayBaselineAnchor = baselineAnchor.transform;

            GameObject attemptAnchor = new GameObject("AttemptTextAnchor");
            attemptAnchor.transform.SetParent(levelRoot.transform);
            attemptAnchor.transform.localPosition = new Vector3(6f, 4f, 0f); // Default estimated position

            GameObject attemptTextObj = new GameObject("AttemptText");
            attemptTextObj.transform.SetParent(attemptAnchor.transform);
            attemptTextObj.transform.localPosition = Vector3.zero;
            TMPro.TextMeshPro tmp = attemptTextObj.AddComponent<TMPro.TextMeshPro>();
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.fontSize = 8;
            tmp.sortingOrder = 100;
            tmp.gameObject.SetActive(false); // Hidden until level starts
            
            def.AttemptText = tmp;
            def.SpeedMultiplier = 1.0f + (levelIndex * 0.05f); // Rough estimate

            // Determine difficulty & theme
            string platformPrefabName = "Platform_Purple";
            if (levelIndex >= 6 && levelIndex <= 10) platformPrefabName = "Platform_Green";
            if (levelIndex > 10) platformPrefabName = "Platform_Green"; // Darker toxic

            GameObject platformPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Environment/{platformPrefabName}.prefab");
            GameObject spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Hazard_Spike.prefab");
            GameObject sawPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Hazard_Saw.prefab");
            GameObject liquidPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Hazard_ToxicLiquid.prefab");
            GameObject endTriggerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/LevelEndTrigger.prefab");

            if (platformPrefab == null)
            {
                Debug.LogError("Art Prefabs not generated! Run Art Generator first.");
                Object.DestroyImmediate(levelRoot);
                return;
            }

            // Procedural generation parameters
            int lengthTokens = 20 + (levelIndex * 2); // 22 to 50
            def.EstimatedLength = lengthTokens * 0.5f;

            float currentX = 0f;
            float currentY = 0f;

            // Safe start zone
            currentX += CreatePlatformBlock(levelRoot, platformPrefab, currentX, currentY, 5f);

            // Generate tokens
            float minY = currentY;
            for (int i = 0; i < lengthTokens; i++)
            {
                int pattern = GetRandomPattern(levelIndex);
                ApplyPattern(pattern, levelRoot, platformPrefab, spikePrefab, sawPrefab, liquidPrefab, ref currentX, ref currentY);
                if (currentY < minY) minY = currentY;
            }

            // Safe end zone
            currentX += CreatePlatformBlock(levelRoot, platformPrefab, currentX, currentY, 5f);

            // Level End Trigger
            GameObject trigger = (GameObject)PrefabUtility.InstantiatePrefab(endTriggerPrefab);
            trigger.transform.SetParent(levelRoot.transform);
            trigger.transform.localPosition = new Vector3(currentX - 1f, currentY + 1f, 0);

            // Set end anchor slightly past the trigger, strictly on the Y=0 baseline
            endAnchor.transform.localPosition = new Vector3(currentX + 4f, 0f, 0);

            // Add Fall Death Zone
            GameObject fallZone = new GameObject("FallDeathZone");
            fallZone.transform.SetParent(levelRoot.transform);
            // Place it 10 units below the lowest platform
            fallZone.transform.localPosition = new Vector3(currentX / 2f, minY - 10f, 0);
            
            BoxCollider2D fallCollider = fallZone.AddComponent<BoxCollider2D>();
            fallCollider.isTrigger = true;
            // Span the entire level plus a 20-unit buffer on both sides
            fallCollider.size = new Vector2(currentX + 40f, 5f);
            
            fallZone.AddComponent<FallDeathZone>();
            // Use Hazard layer so player can theoretically collide with it safely via other systems, 
            // though the script itself explicitly calls Die().
            fallZone.layer = LayerMask.NameToLayer("Hazard") != -1 ? LayerMask.NameToLayer("Hazard") : 0;

            // Save prefab
            string path = $"Assets/Prefabs/Levels/Level_{levelIndex:00}.prefab";
            PrefabUtility.SaveAsPrefabAsset(levelRoot, path);
            Object.DestroyImmediate(levelRoot);
        }

        private static int GetRandomPattern(int level)
        {
            if (level <= 5) return Random.Range(0, 4); // Easy patterns
            if (level <= 7) return Random.Range(1, 6); // Hard patterns
            if (level <= 9) return Random.Range(3, 8); // Very hard patterns
            return Random.Range(5, 10); // Extreme patterns
        }

        private static void ApplyPattern(int pattern, GameObject root, GameObject plat, GameObject spike, GameObject saw, GameObject liquid, ref float x, ref float y)
        {
            switch (pattern)
            {
                case 0: // Flat run
                    x += CreatePlatformBlock(root, plat, x, y, 4f);
                    break;
                case 1: // Small gap
                    x += 2f;
                    x += CreatePlatformBlock(root, plat, x, y, 3f);
                    break;
                case 2: // Single spike
                    CreateHazard(root, spike, x + 1.5f, y + 0.5f);
                    x += CreatePlatformBlock(root, plat, x, y, 4f);
                    break;
                case 3: // Step up
                    x += 2.5f; // gap
                    y += 1f; // step up
                    x += CreatePlatformBlock(root, plat, x, y, 4f);
                    break;
                case 4: // Step down
                    x += 2f;
                    y -= 1f;
                    x += CreatePlatformBlock(root, plat, x, y, 4f);
                    break;
                case 5: // Double spike
                    CreateHazard(root, spike, x + 1.5f, y + 0.5f);
                    CreateHazard(root, spike, x + 2.5f, y + 0.5f);
                    x += CreatePlatformBlock(root, plat, x, y, 5f);
                    break;
                case 6: // Toxic pit
                    CreateToxicPit(root, liquid, x, y - 2f, 4f);
                    x += 4f;
                    x += CreatePlatformBlock(root, plat, x, y, 4f);
                    break;
                case 7: // High step up + spike
                    x += 2.5f; // smaller gap because higher jump
                    y += 2f; // max height
                    x += CreatePlatformBlock(root, plat, x, y, 2f);
                    CreateHazard(root, spike, x - 0.5f, y + 0.5f); // spike right after landing
                    x += CreatePlatformBlock(root, plat, x, y, 2f);
                    break;
                case 8: // Saw gap
                    CreateHazard(root, saw, x + 1.5f, y + 1f); // floating saw
                    x += 3.5f;
                    x += CreatePlatformBlock(root, plat, x, y, 3f);
                    break;
                case 9: // Quick rhythm up down
                    x += 2f;
                    y += 1f;
                    x += CreatePlatformBlock(root, plat, x, y, 2f);
                    x += 2f;
                    y -= 2f;
                    x += CreatePlatformBlock(root, plat, x, y, 2f);
                    x += 2f;
                    y += 1f;
                    x += CreatePlatformBlock(root, plat, x, y, 3f);
                    break;
            }
        }

        private static float CreatePlatformBlock(GameObject root, GameObject prefab, float startX, float y, float width)
        {
            GameObject plat = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            plat.transform.SetParent(root.transform);
            
            // Adjust position so the top-left edge aligns with startX and y
            // Our prefab has origin at center, width 1, height 1
            // So we scale it, and move it down/right
            plat.transform.localPosition = new Vector3(startX + (width / 2f), y - 0.5f, 0);
            
            SpriteRenderer[] srs = plat.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in srs)
            {
                sr.size = new Vector2(width, sr.size.y);
            }
            
            BoxCollider2D col = plat.GetComponent<BoxCollider2D>();
            col.size = new Vector2(width, 1f);

            return width;
        }

        private static void CreateHazard(GameObject root, GameObject prefab, float x, float y)
        {
            GameObject haz = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            haz.transform.SetParent(root.transform);
            haz.transform.localPosition = new Vector3(x, y, 0);
        }

        private static void CreateToxicPit(GameObject root, GameObject prefab, float startX, float y, float width)
        {
            GameObject liq = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            liq.transform.SetParent(root.transform);
            liq.transform.localPosition = new Vector3(startX + (width / 2f), y, 0);
            
            SpriteRenderer sr = liq.GetComponent<SpriteRenderer>();
            sr.size = new Vector2(width, sr.size.y);
            
            BoxCollider2D col = liq.GetComponent<BoxCollider2D>();
            col.size = new Vector2(width, col.size.y);
        }
    }
}
