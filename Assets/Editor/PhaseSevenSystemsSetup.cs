using UnityEngine;
using UnityEditor;
using GeometryDash.Level;
using TMPro;

namespace GeometryDash.Editor
{
    public static class PhaseSevenSystemsSetup
    {
        [MenuItem("Tools/Geometry Dash/7. Run Phase 7 Systems Setup")]
        public static void SetupSystems()
        {
            ConfigureLevelPrefabs();
            InjectTrackerIntoScene();
            Debug.Log("[Phase 7] Speed and Attempt systems setup successfully!");
        }

        private static void InjectTrackerIntoScene()
        {
            string scenePath = "Assets/Scenes/TestScene.unity";
            UnityEngine.SceneManagement.Scene currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            bool wasOpen = currentScene.path == scenePath;
            
            if (!wasOpen)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            }

            LevelSequenceManager lsm = Object.FindObjectOfType<LevelSequenceManager>();
            if (lsm != null)
            {
                AttemptTracker tracker = lsm.gameObject.GetComponent<AttemptTracker>();
                if (tracker == null) tracker = lsm.gameObject.AddComponent<AttemptTracker>();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(lsm.gameObject.scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(lsm.gameObject.scene);
            }
        }

        private static void ConfigureLevelPrefabs()
        {
            float[] multipliers = new float[] {
                0.80f, 0.80f, 0.90f, 0.95f, 1.00f,
                1.05f, 1.10f, 1.15f, 1.20f, 1.25f,
                1.30f, 1.35f, 1.40f, 1.45f, 1.50f
            };

            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                LevelDefinition def = instance.GetComponent<LevelDefinition>();
                if (def != null)
                {
                    def.SpeedMultiplier = multipliers[i - 1];

                    Transform anchor = instance.transform.Find("AttemptTextAnchor");
                    if (anchor == null)
                    {
                        GameObject anchorObj = new GameObject("AttemptTextAnchor");
                        anchorObj.transform.SetParent(instance.transform, false);
                        anchor = anchorObj.transform;
                    }
                    
                    // Create AttemptText component if it doesn't exist
                    Transform attemptTextObj = anchor.Find("AttemptText");
                    if (attemptTextObj == null)
                    {
                        GameObject obj = new GameObject("AttemptText");
                        obj.transform.SetParent(anchor, false);
                        attemptTextObj = obj.transform;
                    }

                    TMPro.TextMeshPro tmp = attemptTextObj.GetComponent<TMPro.TextMeshPro>();
                    if (tmp == null)
                    {
                        tmp = attemptTextObj.gameObject.AddComponent<TMPro.TextMeshPro>();
                        tmp.alignment = TMPro.TextAlignmentOptions.Center;
                        tmp.fontStyle = TMPro.FontStyles.Bold;
                        tmp.fontSize = 8;
                        tmp.sortingOrder = 100;
                    }
                    
                    // Keep it disabled by default so preloads don't show it prematurely
                    attemptTextObj.gameObject.SetActive(false);
                    def.AttemptText = tmp;
                    
                    // Place the text above the starting area
                    float startX = def.StartAnchor != null ? def.StartAnchor.localPosition.x : 0f;
                    float startY = def.GameplayBaselineAnchor != null ? def.GameplayBaselineAnchor.localPosition.y : 0f;
                    anchor.localPosition = new Vector3(startX + 6f, startY + 4f, 0f);
                    attemptTextObj.localPosition = Vector3.zero;
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
