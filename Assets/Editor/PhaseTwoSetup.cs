using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class PhaseTwoSetup
    {
        [MenuItem("Tools/Geometry Dash/Run All Phase 2 Generation")]
        public static void RunAll()
        {
            PhaseTwoArtGenerator.GenerateArtAndPrefabs();
            
            // Wait for prefabs to exist before generating levels
            EditorApplication.delayCall += () =>
            {
                PhaseTwoLevelGenerator.GenerateLevels();
                SetupScene();
            };
        }

        [MenuItem("Tools/Geometry Dash/3. Setup Phase 2 Scene")]
        public static void SetupScene()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            
            // 1. Destroy old test environment (Ground layer cubes)
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var go in rootObjects)
            {
                if (go.name == "Floor" || go.name == "Hazard" || go.name == "Wall")
                {
                    Object.DestroyImmediate(go);
                }
            }

            // 2. Setup LevelSequenceManager
            LevelSequenceManager manager = Object.FindObjectOfType<LevelSequenceManager>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject("LevelSequenceManager");
                manager = managerObj.AddComponent<LevelSequenceManager>();
            }

            manager.LevelPrefabs = new GameObject[15];
            for (int i = 0; i < 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i + 1:00}.prefab";
                GameObject levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (levelPrefab == null)
                {
                    Debug.LogError($"Level {i + 1} not found! Run the level generator first.");
                }
                manager.LevelPrefabs[i] = levelPrefab;
            }

            // Move the player to origin if exists
            GeometryDash.Player.CubePlayerController player = Object.FindObjectOfType<GeometryDash.Player.CubePlayerController>();
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1.5f, 0); // Start slightly above the origin platform
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log("[Phase 2] Scene setup complete! Press Play to start.");
        }
    }
}
