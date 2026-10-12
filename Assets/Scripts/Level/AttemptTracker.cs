using UnityEngine;
using TMPro;
using GeometryDash.Player;

namespace GeometryDash.Level
{
    public class AttemptTracker : MonoBehaviour
    {
        [Header("ATTEMPT DISPLAY")]
        public bool EnableAttemptDisplay = true;
        [Header("TEXT STYLE SETTINGS")]
        public int FontSize = 8; // Sensible size for world space text
        public Color TextColor = Color.white;
        public float OutlineWidth = 0.2f;
        public Color OutlineColor = Color.black;

        private void OnEnable()
        {
            LevelSequenceManager.OnLevelAttemptStarted += HandleLevelAttemptStarted;
        }

        private void OnDisable()
        {
            LevelSequenceManager.OnLevelAttemptStarted -= HandleLevelAttemptStarted;
        }

        private void HandleLevelAttemptStarted(LevelDefinition level)
        {
            if (level == null) return;

            // 1. Update Speed
            CubePlayerController player = FindObjectOfType<CubePlayerController>();
            if (player != null)
            {
                player.SetLevelSpeedMultiplier(level.SpeedMultiplier);
            }

            // 2. Increment Attempt
            int levelId = level.LevelIndex;
            string prefKey = $"LevelAttempt_{levelId}";
            int attempts = PlayerPrefs.GetInt(prefKey, 0);
            attempts++;
            PlayerPrefs.SetInt(prefKey, attempts);
            PlayerPrefs.Save();

            // 3. Display Text
            if (EnableAttemptDisplay && level.AttemptText != null)
            {
                level.AttemptText.text = $"ATTEMPT {attempts}";
                level.AttemptText.fontSize = FontSize;
                level.AttemptText.color = TextColor;
                level.AttemptText.outlineWidth = OutlineWidth;
                level.AttemptText.outlineColor = OutlineColor;
                
                // Ensure it renders above level
                level.AttemptText.sortingOrder = 100;
                
                // Make it visible
                level.AttemptText.gameObject.SetActive(true);
                level.AttemptText.enabled = true;
                
                Debug.Log($"[ATTEMPT] Level {levelId:00} count = {attempts}. Text assigned = {level.AttemptText.text}");
            }
        }
    }
}
