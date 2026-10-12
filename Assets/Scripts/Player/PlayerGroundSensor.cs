using UnityEngine;

namespace GeometryDash.Player
{
    public class PlayerGroundSensor : MonoBehaviour
    {
        [Header("GROUND CHECK")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckHeight = 0.1f;
        [SerializeField] private float widthMultiplier = 0.9f;

        public bool IsGrounded { get; private set; }
        
        private BoxCollider2D playerCollider;
        private bool debuggedOnce = false;

        public void ForceUnground()
        {
            IsGrounded = false;
        }

        private void Awake()
        {
            playerCollider = GetComponentInParent<BoxCollider2D>();
            
            // Failsafe: if the layer mask is 0 (Nothing), force it to include the Ground layer
            if (groundLayer.value == 0)
            {
                int layer = LayerMask.NameToLayer("Ground");
                if (layer != -1) groundLayer.value = 1 << layer;
            }
        }

        public void CheckGround()
        {
            if (playerCollider == null) return;

            Bounds bounds = playerCollider.bounds;
            Vector2 checkSize = new Vector2(bounds.size.x * widthMultiplier, groundCheckHeight);
            // Place exactly below the collider
            Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - (groundCheckHeight * 0.5f));

            Collider2D hit = Physics2D.OverlapBox(checkPosition, checkSize, 0f, groundLayer);
            IsGrounded = hit != null;

            if (!debuggedOnce)
            {
                Debug.Log($"[GROUND] IsGrounded: {IsGrounded}");
                Debug.Log($"[GROUND] LayerMask value: {groundLayer.value}");
                Debug.Log($"[GROUND] Player Bottom Y: {bounds.min.y}");
                Debug.Log($"[GROUND] Check Position: {checkPosition}");
                if (hit != null) Debug.Log($"[GROUND] Hit: {hit.gameObject.name} (Layer: {hit.gameObject.layer})");
                debuggedOnce = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerCollider == null) playerCollider = GetComponentInParent<BoxCollider2D>();
            if (playerCollider == null) return;

            Bounds bounds = playerCollider.bounds;
            Vector2 checkSize = new Vector2(bounds.size.x * widthMultiplier, groundCheckHeight);
            Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - (groundCheckHeight * 0.5f));

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(checkPosition, checkSize);
        }
    }
}
