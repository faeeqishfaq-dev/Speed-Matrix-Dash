using UnityEngine;

namespace GeometryDash.Level
{
    [DefaultExecutionOrder(200)] // Ensure it runs after normal camera follow
    public class MilestoneCameraClamp : MonoBehaviour
    {
        public UnityEngine.Camera cam;
        
        [Header("Manual Overrides (Leave alone)")]
        public float finishWallRightEdge;
        public float wallEdgeOffset = 0f;

        [Header("Milestone Clamping")]
        [Tooltip("How much of the purple wall is allowed to become visible before the camera stops")]
        public float wallRevealAmount = 0.1f;
        
        private bool clampEnabled = false;
        private float finishWallLeftEdge;
        private bool hasLoggedStop = false;

        private void Start()
        {
            if (cam == null) cam = UnityEngine.Camera.main;
        }

        public void EnableClampDynamic(float leftEdge)
        {
            finishWallLeftEdge = leftEdge;
            clampEnabled = true;
            hasLoggedStop = false;
            
            if (cam == null) cam = UnityEngine.Camera.main;

            float halfWidth = cam.orthographicSize * cam.aspect;
            float maxCamX = finishWallLeftEdge + wallRevealAmount - halfWidth;

            Debug.Log($"[MILESTONE CLAMP]\n" +
                      $"Wall Left Edge = {finishWallLeftEdge}\n" +
                      $"Camera Half Width = {halfWidth}\n" +
                      $"Wall Reveal Amount = {wallRevealAmount}\n" +
                      $"Calculated Max Camera X = {maxCamX}");
        }

        public void DisableClamp()
        {
            clampEnabled = false;
        }

        private void LateUpdate()
        {
            if (!clampEnabled || cam == null)
                return;

            float halfWidth = cam.orthographicSize * cam.aspect;
            float maxCameraX = finishWallLeftEdge + wallRevealAmount - halfWidth;

            Vector3 pos = cam.transform.position;

            if (pos.x > maxCameraX)
            {
                if (!hasLoggedStop)
                {
                    Debug.Log($"[MILESTONE CLAMP] Camera stopped at FinishWall. Max X = {maxCameraX}");
                    hasLoggedStop = true;
                }
                pos.x = maxCameraX;
            }

            cam.transform.position = pos;
        }

        private void OnDrawGizmos()
        {
            if (clampEnabled && cam != null)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                float maxCameraX = finishWallLeftEdge + wallRevealAmount - halfWidth;

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(new Vector3(finishWallLeftEdge, -10, 0), new Vector3(finishWallLeftEdge, 10, 0));

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(new Vector3(maxCameraX, -10, 0), new Vector3(maxCameraX, 10, 0));
            }
        }
    }
}
