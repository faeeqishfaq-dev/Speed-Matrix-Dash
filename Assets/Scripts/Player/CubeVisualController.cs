using UnityEngine;

namespace GeometryDash.Player
{
    public class CubeVisualController : MonoBehaviour
    {
        [Header("VISUAL ROTATION")]
        [Tooltip("How many degrees the cube rotates for EVERY successful jump.")]
        [SerializeField] private float rotationPerJump = 180f;

        [Tooltip("Time required to visually rotate 180 degrees.")]
        [SerializeField] private float rotationDurationPerJump = 0.22f;

        [Tooltip("Clockwise while moving right.")]
        [SerializeField] private bool clockwise = true;

        private CubePlayerController playerController;

        private float currentRotation;
        private float targetRotation;

        private void Awake()
        {
            playerController = GetComponentInParent<CubePlayerController>();
        }

        private void OnEnable()
        {
            if (playerController == null)
                playerController = GetComponentInParent<CubePlayerController>();

            if (playerController != null)
            {
                // Prevent duplicate subscription.
                playerController.OnJumpStarted -= HandleJumpStarted;
                playerController.OnJumpStarted += HandleJumpStarted;
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
                playerController.OnJumpStarted -= HandleJumpStarted;
        }

        /// <summary>
        /// Called for EVERY successful physical jump.
        ///
        /// Single jump:
        /// +180 degrees
        ///
        /// Double jump:
        /// First jump +180
        /// Second jump +180
        /// = 360 total
        /// </summary>
        private void HandleJumpStarted()
        {
            if (playerController == null || !playerController.IsAlive)
                return;

            float direction = clockwise ? -1f : 1f;

            // IMPORTANT:
            // Never reset the previous rotation here.
            // Every successful jump simply adds another 180 degrees.
            targetRotation += rotationPerJump * direction;

            Debug.Log(
                $"[VISUAL] Successful Jump -> Target Rotation: {targetRotation}"
            );
        }

        private void Update()
        {
            if (playerController == null || !playerController.IsAlive)
                return;

            float duration = Mathf.Max(0.01f, rotationDurationPerJump);

            // How many degrees should be covered per second.
            float rotationSpeed = rotationPerJump / duration;

            currentRotation = Mathf.MoveTowards(
                currentRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            transform.localRotation =
                Quaternion.Euler(0f, 0f, currentRotation);
        }

        /// <summary>
        /// Call when the player fully respawns/restarts.
        /// </summary>
        public void ResetVisualState()
        {
            currentRotation = 0f;
            targetRotation = 0f;

            transform.localRotation = Quaternion.identity;

            Debug.Log("[VISUAL] Rotation state reset for respawn.");
        }
    }
}