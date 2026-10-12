using UnityEngine;
using UnityEditor;

namespace GeometryDash.Editor
{
    public static class LevelAlignmentFixer
    {
        [MenuItem("Tools/Geometry Dash/RECOVERY: Align All Levels Horizontally")]
        public static void FixAlignment()
        {
            int fixedCount = 0;
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                bool modified = false;

                // 1. Determine Gameplay Baseline (First Ground Platform)
                BoxCollider2D[] allColliders = instance.GetComponentsInChildren<BoxCollider2D>();
                BoxCollider2D firstPlatform = null;
                float minX = float.MaxValue;

                foreach (var col in allColliders)
                {
                    if (col.isTrigger) continue;
                    
                    if (col.bounds.min.x < minX)
                    {
                        minX = col.bounds.min.x;
                        firstPlatform = col;
                    }
                }

                if (firstPlatform != null)
                {
                    float baselineLocalY = firstPlatform.bounds.max.y; // Because instance is at 0,0,0

                    // 2. Configure GameplayBaselineAnchor
                    Transform baselineAnchor = instance.transform.Find("GameplayBaselineAnchor");
                    if (baselineAnchor == null)
                    {
                        GameObject anchorObj = new GameObject("GameplayBaselineAnchor");
                        anchorObj.transform.SetParent(instance.transform, false);
                        baselineAnchor = anchorObj.transform;
                    }
                    baselineAnchor.localPosition = new Vector3(0f, baselineLocalY, 0f);

                    // 3. Configure StartAnchor
                    Transform startAnchor = instance.transform.Find("StartAnchor");
                    if (startAnchor != null)
                    {
                        startAnchor.localPosition = new Vector3(startAnchor.localPosition.x, baselineLocalY, 0f);
                    }

                    // 4. Configure EndAnchor
                    Transform endAnchor = instance.transform.Find("EndAnchor");
                    if (endAnchor != null)
                    {
                        endAnchor.localPosition = new Vector3(endAnchor.localPosition.x, baselineLocalY, 0f);
                    }

                    modified = true;
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    fixedCount++;
                }
                
                Object.DestroyImmediate(instance);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[RECOVERY] Successfully mapped Baseline Anchors for {fixedCount} levels! All levels will now perfectly align their start floor to the global Y.");
        }
    }
}
