using UnityEngine;
using UnityEditor;

namespace GeometryDash.Editor
{
    public static class FixLevelEnds
    {
        [MenuItem("Tools/Geometry Dash/RECOVERY: Fix Level Connections")]
        public static void FixEnds()
        {
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                
                // Find the EndAnchor
                Transform endAnchor = instance.transform.Find("EndAnchor");
                if (endAnchor != null)
                {
                    // Find the platform closest to EndAnchor
                    BoxCollider2D[] cols = instance.GetComponentsInChildren<BoxCollider2D>();
                    BoxCollider2D lastPlatform = null;
                    float minDiff = float.MaxValue;
                    
                    foreach(var col in cols)
                    {
                        if (col.isTrigger) continue;
                        // Assuming platform is long, we check bounds.max.x
                        float diff = Mathf.Abs(col.bounds.max.x - endAnchor.position.x);
                        if (diff < minDiff && diff < 15f)
                        {
                            minDiff = diff;
                            lastPlatform = col;
                        }
                    }
                    
                    if (lastPlatform != null)
                    {
                        float currentTopY = lastPlatform.bounds.max.y;
                        float offset = 0f - currentTopY;
                        
                        lastPlatform.transform.position = new Vector3(
                            lastPlatform.transform.position.x, 
                            lastPlatform.transform.position.y + offset, 
                            lastPlatform.transform.position.z);
                            
                        endAnchor.position = new Vector3(endAnchor.position.x, 0, 0);
                    }
                }
                
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("[RECOVERY] Level connections fixed! All levels now end exactly at Y=0 to match the start.");
        }
    }
}
