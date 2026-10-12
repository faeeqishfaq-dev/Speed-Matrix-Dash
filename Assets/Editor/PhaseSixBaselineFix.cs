using UnityEngine;
using UnityEditor;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class PhaseSixBaselineFix
    {
        [MenuItem("Tools/Geometry Dash/6. Run Phase 6 Baseline Fix")]
        public static void FixLevelBaselines()
        {
            int updatedCount = 0;
            
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                
                LevelDefinition def = instance.GetComponent<LevelDefinition>();
                if (def != null)
                {
                    // Find the very first solid ground collider (lowest X)
                    BoxCollider2D[] allColliders = instance.GetComponentsInChildren<BoxCollider2D>();
                    BoxCollider2D firstGround = null;
                    float minX = float.MaxValue;

                    foreach (var col in allColliders)
                    {
                        if (col.isTrigger) continue; // Skip hazards/triggers
                        
                        // We assume starting safe ground is early in X
                        float currentMinX = col.bounds.min.x;
                        if (currentMinX < minX)
                        {
                            minX = currentMinX;
                            firstGround = col;
                        }
                    }

                    if (firstGround != null)
                    {
                        // Extract top surface Y
                        float topSurfaceY = firstGround.bounds.max.y;

                        // Add/Update Anchor
                        Transform anchorTransform = instance.transform.Find("GameplayBaselineAnchor");
                        if (anchorTransform == null)
                        {
                            GameObject anchorObj = new GameObject("GameplayBaselineAnchor");
                            anchorObj.transform.SetParent(instance.transform, false);
                            anchorTransform = anchorObj.transform;
                            def.GameplayBaselineAnchor = anchorTransform;
                        }
                        
                        anchorTransform.localPosition = new Vector3(0f, topSurfaceY, 0f);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase 6] GameplayBaselineAnchor configured for {updatedCount} levels!");
        }
    }
}
