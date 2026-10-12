using UnityEngine;

namespace GeometryDash.Level
{
    public class LevelDefinition : MonoBehaviour
    {
        [Header("METADATA")]
        public int LevelIndex;
        public string Difficulty;
        public float EstimatedLength;

        [Header("BACKGROUND")]
        [Tooltip("The background sprite to display while this level is active.")]
        public Sprite BackgroundSprite;

        [Header("LEVEL SPEED")]
        [Tooltip("The speed multiplier applied to the Cube when this level is active.")]
        public float SpeedMultiplier = 1.0f;

        [Header("ATTEMPT DISPLAY")]
        [Tooltip("The TextMeshPro component in this level to display the attempt count.")]
        public TMPro.TextMeshPro AttemptText;

        [Header("ANCHORS")]
        [Tooltip("The point where the player visually enters the level. Matches the previous level's EndAnchor.")]
        public Transform StartAnchor;
        
        [Tooltip("The point where the level visually ends. Matches the next level's StartAnchor.")]
        public Transform EndAnchor;

        [Tooltip("The exact Y coordinate of the normal starting safe ground surface.")]
        public Transform GameplayBaselineAnchor;
    }
}
