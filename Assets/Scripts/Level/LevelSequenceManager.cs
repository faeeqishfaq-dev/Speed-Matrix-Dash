using UnityEngine;
using System.Collections.Generic;
using GeometryDash.Player;

namespace GeometryDash.Level
{
    public class LevelSequenceManager : MonoBehaviour
    {
        [Header("LEVELS")]
        [Tooltip("The ordered array of 15 level prefabs to stream sequentially.")]
        public GameObject[] LevelPrefabs;

        [Header("PLAYER")]
        [Tooltip("Optional reference to the player. Used to track position if necessary for advanced streaming.")]
        public CubePlayerController Player;

        private GameObject currentLevelInstance;
        private GameObject nextLevelInstance;
        
        public static int savedLevelIndex = 0;
        private int currentLevelIndex = 0;
        private float globalGameplayBaselineY = 0f;

        public static event System.Action<LevelDefinition> OnLevelAttemptStarted;

        private void Start()
        {
            currentLevelIndex = savedLevelIndex;
            
            if (LevelPrefabs == null || LevelPrefabs.Length == 0)
            {
                Debug.LogError("[LEVEL SEQUENCE] No level prefabs assigned.");
                return;
            }

            // Define global baseline from Level 1
            LevelDefinition firstDef = LevelPrefabs[0].GetComponent<LevelDefinition>();
            if (firstDef != null && firstDef.GameplayBaselineAnchor != null)
            {
                globalGameplayBaselineY = firstDef.GameplayBaselineAnchor.localPosition.y;
            }

            // Start by loading the saved level at the origin X
            currentLevelInstance = InstantiateLevel(currentLevelIndex, 0f);
            SetupLevelTrigger(currentLevelInstance);
            
            // Preload the next level immediately
            PreloadNextLevel();

            // Trigger the level attempt started event for the first active level
            if (currentLevelInstance != null)
            {
                OnLevelAttemptStarted?.Invoke(currentLevelInstance.GetComponent<LevelDefinition>());
            }
        }

        private GameObject InstantiateLevel(int index, float targetStartX)
        {
            if (index < 0 || index >= LevelPrefabs.Length) return null;

            GameObject prefab = LevelPrefabs[index];
            if (prefab == null) return null;

            GameObject instance = Instantiate(prefab);
            instance.name = $"Level_{index + 1}";
            
            LevelDefinition def = instance.GetComponent<LevelDefinition>();
            if (def != null)
            {
                float nextRootX = targetStartX;
                if (def.StartAnchor != null)
                {
                    nextRootX = targetStartX - def.StartAnchor.localPosition.x;
                }

                float requiredRootY = 0f;
                if (def.GameplayBaselineAnchor != null)
                {
                    float localBaselineY = def.GameplayBaselineAnchor.localPosition.y;
                    requiredRootY = globalGameplayBaselineY - localBaselineY;
                }

                instance.transform.position = new Vector3(nextRootX, requiredRootY, 0f);

                // TEMPORARY LOGGING AS REQUESTED
                if (def.GameplayBaselineAnchor != null)
                {
                    Debug.Log($"{instance.name}:\n" +
                              $"Local Baseline = {def.GameplayBaselineAnchor.localPosition.y}\n" +
                              $"Root Y = {requiredRootY}\n" +
                              $"World Baseline = {def.GameplayBaselineAnchor.position.y}");
                }
            }
            else
            {
                instance.transform.position = new Vector3(targetStartX, 0f, 0f);
            }
            
            return instance;
        }

        private void PreloadNextLevel()
        {
            int nextIndex = currentLevelIndex + 1;
            if (nextIndex >= LevelPrefabs.Length)
            {
                Debug.Log("[LEVEL SEQUENCE] Reached the end of the game! No more levels to preload.");
                return; // Reached the end
            }

            int currentNum = currentLevelIndex + 1;
            if (currentNum == 4 || currentNum == 8 || currentNum == 12)
            {
                // Milestone transition: Do not preload automatically.
                return;
            }

            ForcePreloadNextLevel();
        }

        private void ForcePreloadNextLevel()
        {
            int nextIndex = currentLevelIndex + 1;
            if (nextIndex >= LevelPrefabs.Length) return;

            LevelDefinition currentDef = currentLevelInstance.GetComponent<LevelDefinition>();
            if (currentDef == null || currentDef.EndAnchor == null)
            {
                Debug.LogError("[LEVEL SEQUENCE] Current level is missing an EndAnchor! Cannot preload safely.");
                return;
            }

            nextLevelInstance = InstantiateLevel(nextIndex, currentDef.EndAnchor.position.x);
            SetupLevelTrigger(nextLevelInstance);
        }

        private void SetupLevelTrigger(GameObject levelInstance)
        {
            if (levelInstance == null) return;
            LevelEndTrigger trigger = levelInstance.GetComponentInChildren<LevelEndTrigger>();
            if (trigger != null)
            {
                trigger.OnLevelCompleted += HandleLevelCompleted;
            }
        }

        private void HandleLevelCompleted()
        {
            Debug.Log($"[LEVEL SEQUENCE] Level {currentLevelIndex + 1} completed!");
            LevelProgressManager.MarkLevelCompleted(currentLevelIndex + 1);
            AdvanceToNextLevel();
        }

        public GameObject GetNextLevelInstance()
        {
            return nextLevelInstance;
        }

        public void AdvanceToNextLevel()
        {
            if (nextLevelInstance == null)
            {
                ForcePreloadNextLevel();
            }

            Destroy(currentLevelInstance, 3f);

            currentLevelIndex++;
            savedLevelIndex = currentLevelIndex;
            
            currentLevelInstance = nextLevelInstance;
            nextLevelInstance = null;

            if (currentLevelInstance != null)
            {
                OnLevelAttemptStarted?.Invoke(currentLevelInstance.GetComponent<LevelDefinition>());
            }

            if (currentLevelIndex >= LevelPrefabs.Length)
            {
                Debug.Log("======================================");
                Debug.Log("ALL 15 LEVELS COMPLETE! YOU BEAT THE GAME!");
                Debug.Log("======================================");
            }
            else
            {
                PreloadNextLevel();
            }
        }

        private void OnDrawGizmos()
        {
            // Draw global baseline line
            Gizmos.color = Color.green;
            Vector3 left = new Vector3(-1000f, globalGameplayBaselineY, 0f);
            Vector3 right = new Vector3(1000f, globalGameplayBaselineY, 0f);
            Gizmos.DrawLine(left, right);

            // Draw a marker at global baseline to identify it
            Gizmos.DrawWireCube(new Vector3(UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform.position.x : 0f, globalGameplayBaselineY, 0f), new Vector3(5f, 0.1f, 0f));

            // Draw markers for all loaded baseline anchors
            Gizmos.color = Color.magenta;
            if (currentLevelInstance != null)
            {
                LevelDefinition def = currentLevelInstance.GetComponent<LevelDefinition>();
                if (def != null && def.GameplayBaselineAnchor != null)
                {
                    Gizmos.DrawSphere(def.GameplayBaselineAnchor.position, 0.3f);
                }
            }
            if (nextLevelInstance != null)
            {
                LevelDefinition def = nextLevelInstance.GetComponent<LevelDefinition>();
                if (def != null && def.GameplayBaselineAnchor != null)
                {
                    Gizmos.DrawSphere(def.GameplayBaselineAnchor.position, 0.3f);
                }
            }
        }
    }
}
