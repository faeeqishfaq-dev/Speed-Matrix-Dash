using UnityEngine;
using UnityEditor;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class FallDeathZoneInjector
    {
        [MenuItem("Tools/Geometry Dash/Inject Fall Death Zones")]
        public static void InjectZones()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            int updatedCount = 0;

            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                // Open prefab by instantiating it at origin
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = Vector3.zero;

                // Remove existing FallDeathZone if it already exists
                Transform existing = instance.transform.Find("FallDeathZone");
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                // Calculate horizontal boundaries based on anchors
                float minX = 0f;
                float maxX = 0f;
                LevelDefinition def = instance.GetComponent<LevelDefinition>();
                if (def != null)
                {
                    if (def.StartAnchor != null) minX = def.StartAnchor.localPosition.x;
                    if (def.EndAnchor != null) maxX = def.EndAnchor.localPosition.x;
                }

                // Find lowest solid ground/hazard point
                float minY = 0f;
                bool foundAny = false;
                
                BoxCollider2D[] colliders = instance.GetComponentsInChildren<BoxCollider2D>();
                foreach (var col in colliders)
                {
                    if (col.isTrigger) continue; // Skip existing triggers

                    // Check bounds in world space (which is local space since root is at 0,0,0)
                    float bottom = col.bounds.min.y;
                    
                    if (!foundAny || bottom < minY)
                    {
                        minY = bottom;
                        foundAny = true;
                    }
                }
                
                // If the level has no platforms, fallback
                if (!foundAny) minY = -5f;

                // Configure FallDeathZone properties
                float width = Mathf.Max((maxX - minX) + 40f, 100f); // Ensure it's wide enough and add 20f buffer each side
                float centerX = minX + (maxX - minX) / 2f;
                float targetY = minY - 4f; // 4 units below the lowest valid platform

                // Create the actual GameObject
                GameObject fdz = new GameObject("FallDeathZone");
                fdz.transform.SetParent(instance.transform);
                fdz.transform.localPosition = new Vector3(centerX, targetY, 0);

                // Add Collider
                BoxCollider2D fdzCol = fdz.AddComponent<BoxCollider2D>();
                fdzCol.isTrigger = true;
                fdzCol.size = new Vector2(width, 1.5f); // 1.5 units high

                // Add component
                fdz.AddComponent<FallDeathZone>();

                // Save prefab modifications
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase 2] Successfully injected actual FallDeathZone GameObjects into {updatedCount} Level Prefabs!");
        }
    }
}
