using UnityEngine;
using UnityEditor;

namespace GeometryDash.Editor
{
    public static class FallDeathZoneFixer
    {
        [MenuItem("Tools/Geometry Dash/RECOVERY: Fix Fall Death Zones")]
        public static void FixZones()
        {
            int fixedCount = 0;
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                
                Transform fallZoneTransform = instance.transform.Find("FallDeathZone");
                if (fallZoneTransform != null)
                {
                    BoxCollider2D fallCollider = fallZoneTransform.GetComponent<BoxCollider2D>();
                    if (fallCollider != null)
                    {
                        // Calculate level bounds
                        BoxCollider2D[] allColliders = instance.GetComponentsInChildren<BoxCollider2D>();
                        float minX = float.MaxValue;
                        float maxX = float.MinValue;
                        float minY = float.MaxValue;

                        foreach (var col in allColliders)
                        {
                            if (col == fallCollider) continue;
                            if (col.isTrigger) continue;
                            
                            if (col.bounds.min.x < minX) minX = col.bounds.min.x;
                            if (col.bounds.max.x > maxX) maxX = col.bounds.max.x;
                            if (col.bounds.min.y < minY) minY = col.bounds.min.y;
                        }

                        float width = (maxX - minX) + 40f; // Add a generous buffer
                        float centerX = minX + (maxX - minX) / 2f;
                        
                        // Position safely below the lowest platform
                        float zoneY = minY - 10f;

                        fallZoneTransform.localPosition = new Vector3(centerX, zoneY, 0f);
                        
                        // Fix the collider size (prevent skipping)
                        fallCollider.size = new Vector2(width, 10f); // 10 units thick!
                        fallCollider.isTrigger = true;
                    }

                    // Ensure FallDeathZone.cs is attached properly (no namespace conflicts, just adding if missing)
                    var script = fallZoneTransform.GetComponent<GeometryDash.Level.FallDeathZone>();
                    if (script == null)
                    {
                        fallZoneTransform.gameObject.AddComponent<GeometryDash.Level.FallDeathZone>();
                    }

                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    fixedCount++;
                }

                Object.DestroyImmediate(instance);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[RECOVERY] Successfully fixed FallDeathZone sizes and positions for {fixedCount} levels!");
        }
    }
}
