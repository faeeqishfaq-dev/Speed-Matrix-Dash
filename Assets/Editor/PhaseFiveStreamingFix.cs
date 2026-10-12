using UnityEngine;
using UnityEditor;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class PhaseFiveStreamingFix
    {
        [MenuItem("Tools/Geometry Dash/5. Run Phase 5 Streaming Fix")]
        public static void FixLevelAnchors()
        {
            int updatedCount = 0;
            
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                
                // 1. Ensure prefab root is cleanly zeroed (it normally should be, but just in case)
                instance.transform.position = Vector3.zero;

                // 2. Normalize Anchors
                LevelDefinition def = instance.GetComponent<LevelDefinition>();
                if (def != null)
                {
                    if (def.StartAnchor != null)
                    {
                        Vector3 pos = def.StartAnchor.localPosition;
                        pos.y = 0f;
                        def.StartAnchor.localPosition = pos;
                    }
                    
                    if (def.EndAnchor != null)
                    {
                        Vector3 pos = def.EndAnchor.localPosition;
                        pos.y = 0f;
                        def.EndAnchor.localPosition = pos;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase 5] Normalized Start/End Anchors to Y=0 for {updatedCount} levels!");
        }
    }
}
