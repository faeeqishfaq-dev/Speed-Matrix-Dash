using UnityEngine;
using UnityEditor;

namespace GeometryDash.Editor
{
    public static class CheckLevels 
    {
        public static void Check() 
        {
            for(int i=1; i<=15; i++) 
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                BoxCollider2D[] cols = instance.GetComponentsInChildren<BoxCollider2D>();
                float minX = 9999f, maxX = -9999f;
                BoxCollider2D first = null, last = null;
                foreach(var c in cols) 
                {
                    if(c.isTrigger) continue;
                    if(c.bounds.min.x < minX) { minX = c.bounds.min.x; first = c; }
                    if(c.bounds.max.x > maxX) { maxX = c.bounds.max.x; last = c; }
                }
                
                Transform anchor = instance.transform.Find("GameplayBaselineAnchor");
                float anchorY = anchor != null ? anchor.localPosition.y : 0f;
                
                Debug.Log($"Level {i}: First Top={first.bounds.max.y}, Last Top={last.bounds.max.y}, AnchorY={anchorY}");
                Object.DestroyImmediate(instance);
            }
        }
    }
}
