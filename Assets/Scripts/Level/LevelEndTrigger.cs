using UnityEngine;
using GeometryDash.Player;
using System;

namespace GeometryDash.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelEndTrigger : MonoBehaviour
    {
        public event Action OnLevelCompleted;
        
        private bool hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasTriggered) return;
            
            // If the Milestone completion director has already paused the game, do not trigger normal end.
            if (Time.timeScale <= 0.01f) return;

            CubePlayerController player = collision.GetComponentInParent<CubePlayerController>();
            if (player != null && player.IsAlive)
            {
                // Check if this level has a MilestoneFinishTrigger that the player is also hitting,
                // or if it's a milestone level, we should not fire normal level completion.
                // An easy way is to see if a MilestoneFinishTrigger exists in this level instance.
                MilestoneFinishTrigger milestone = GetComponentInParent<LevelDefinition>()?.GetComponentInChildren<MilestoneFinishTrigger>();
                if (milestone != null)
                {
                    // This is a milestone level. The MilestoneFinishTrigger handles completion!
                    return;
                }

                hasTriggered = true;
                OnLevelCompleted?.Invoke();
            }
        }
    }
}
