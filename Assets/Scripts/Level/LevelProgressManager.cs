using UnityEngine;

namespace GeometryDash.Level
{
    /// <summary>
    /// Manages persistent level unlocking progression.
    /// Default state: Level 1 unlocked, Levels 2-15 locked.
    /// Completing Level N unlocks Level N + 1. Progress is never relocked.
    /// </summary>
    public static class LevelProgressManager
    {
        private const string KEY_HIGHEST_UNLOCKED = "HighestUnlockedLevel";
        public const int TOTAL_LEVELS = 15;

        public static event System.Action<int> OnLevelUnlocked;

        /// <summary>
        /// Returns the highest level number (1-15) that the player has unlocked.
        /// </summary>
        public static int GetHighestUnlockedLevel()
        {
            int highest = PlayerPrefs.GetInt(KEY_HIGHEST_UNLOCKED, 1);
            return Mathf.Clamp(highest, 1, TOTAL_LEVELS);
        }

        /// <summary>
        /// Returns true if the specified 1-based level number is unlocked.
        /// </summary>
        public static bool IsLevelUnlocked(int levelNumber)
        {
            if (levelNumber <= 1) return true;
            return levelNumber <= GetHighestUnlockedLevel();
        }

        /// <summary>
        /// Call when a level is successfully completed to sequentially unlock the next level.
        /// </summary>
        public static void MarkLevelCompleted(int completedLevelNumber)
        {
            if (completedLevelNumber < 1 || completedLevelNumber > TOTAL_LEVELS) return;

            int currentHighest = GetHighestUnlockedLevel();
            int nextLevel = completedLevelNumber + 1;

            if (nextLevel > currentHighest && nextLevel <= TOTAL_LEVELS)
            {
                PlayerPrefs.SetInt(KEY_HIGHEST_UNLOCKED, nextLevel);
                PlayerPrefs.Save();
                Debug.Log($"[LevelProgressManager] Level {completedLevelNumber} completed! Unlocked Level {nextLevel}.");
                OnLevelUnlocked?.Invoke(nextLevel);
            }
            else if (completedLevelNumber == TOTAL_LEVELS)
            {
                Debug.Log("[LevelProgressManager] Final level (Level 15) completed! All levels mastered.");
            }
        }

        /// <summary>
        /// Resets level progress back to Level 1 unlocked only.
        /// </summary>
        public static void ResetProgress()
        {
            PlayerPrefs.SetInt(KEY_HIGHEST_UNLOCKED, 1);
            PlayerPrefs.Save();
            Debug.Log("[LevelProgressManager] Level progress reset to Level 1.");
            OnLevelUnlocked?.Invoke(1);
        }
    }
}
