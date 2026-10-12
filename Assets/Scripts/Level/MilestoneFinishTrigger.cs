using UnityEngine;
using GeometryDash.Player;

namespace GeometryDash.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MilestoneFinishTrigger : MonoBehaviour
    {
        private bool triggered = false;

        private void Start()
        {
            if (UnityEngine.Camera.main != null)
            {
                MilestoneCameraClamp clamp = UnityEngine.Camera.main.GetComponent<MilestoneCameraClamp>();
                if (clamp != null)
                {
                    // Calculate the true world left edge of the wall visual.
                    Transform wallVisual = transform.parent != null ? transform.parent.Find("WallVisual") : transform.Find("WallVisual");
                    if (wallVisual != null)
                    {
                        Renderer renderer = wallVisual.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            float leftEdge = renderer.bounds.min.x;
                            clamp.EnableClampDynamic(leftEdge);
                        }
                    }
                }
            }
            // Ensure MilestoneWallVFXController is attached to root
            MilestoneWallVFXController wallVFX = GetComponentInParent<MilestoneWallVFXController>();
            if (wallVFX == null)
            {
                Transform root = transform.parent != null ? transform.parent : transform;
                root.gameObject.AddComponent<MilestoneWallVFXController>();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (triggered) return;

            CubePlayerController player = collision.GetComponentInParent<CubePlayerController>();
            if (player != null && player.IsAlive)
            {
                triggered = true;

                MilestoneWallVFXController vfx = GetComponentInParent<MilestoneWallVFXController>();
                if (vfx != null) vfx.OnPlayerContact();

                LevelDefinition def = GetComponentInParent<LevelDefinition>();
                if (def != null && def.LevelIndex > 0)
                {
                    LevelProgressManager.MarkLevelCompleted(def.LevelIndex);
                }

                if (MilestoneCompletionDirector.Instance != null)
                {
                    // Pass the wall's position to originate the VFX from this exact wall
                    MilestoneCompletionDirector.Instance.BeginMilestone(transform.position);
                }
            }
        }
    }
}
