using UnityEngine;

namespace GeometryDash.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CubePlayerController : MonoBehaviour
    {
        [Header("PLAYER MOVEMENT")]
        [SerializeField] private float baseMoveSpeed = 8.5f;
        [SerializeField] private float jumpVelocity = 15f;
        [SerializeField] private float gravityScale = 5f;
        [SerializeField] private float maxFallSpeed = -25f;

        [Header("SPEED PROGRESSION")]
        [SerializeField] private bool enableLevelSpeedProgression = true;
        [SerializeField] private float speedTransitionDuration = 0.25f;
        
        private float currentSpeedMultiplier = 1.0f;
        private float targetSpeedMultiplier = 1.0f;
        private float startSpeedMultiplier = 1.0f;
        private float speedTransitionTime = 0f;

        private Rigidbody2D rb;
        private PlayerInputController inputController;
        private PlayerGroundSensor groundSensor;

        private bool isAlive = true;
        public bool IsAlive 
        { 
            get => isAlive; 
            set 
            {
                isAlive = value;
                if (!isAlive)
                {
                    jumpCount = 0;
                    firstJumpTime = -1f;
                    jumpRequestBuffered = false;
                    hasLeftGroundSinceJump = true;
                }
            }
        }
        public bool IsGrounded => groundSensor != null && groundSensor.IsGrounded;
        public float JumpVelocity => jumpVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            inputController = GetComponent<PlayerInputController>();
            groundSensor = GetComponentInChildren<PlayerGroundSensor>();

            // Setup rigidbody for deterministic platformer physics
            rb.gravityScale = gravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Disable interpolation for absolute instant visual response in arcade platformer
            //rb.interpolation = RigidbodyInterpolation2D.None;
            //rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (groundSensor != null)
            {
                groundSensor.CheckGround();
            }
        }



        public void SetLevelSpeedMultiplier(float multiplier)
        {
            targetSpeedMultiplier = multiplier;
            startSpeedMultiplier = currentSpeedMultiplier;
            speedTransitionTime = 0f;
            if (speedTransitionDuration <= 0f) currentSpeedMultiplier = multiplier;
        }

        [Header("DOUBLE JUMP")]
        [SerializeField] private bool enableDoubleJump = true;
        [SerializeField] private float doubleTapWindow = 0.30f;
        [SerializeField] private float secondJumpInputBuffer = 0.10f;
        [SerializeField] private float secondJumpForceMultiplier = 1.0f;

        private int jumpCount = 0;
        private float firstJumpTime = -1f;
        private bool hasLeftGroundSinceJump = true;
        
        private bool jumpRequestBuffered = false;
        private float jumpRequestTime = -1f;

        public event System.Action OnJumpStarted;

        private void Update()
        {
            if (!IsAlive) return;

            // Speed Transition
            if (enableLevelSpeedProgression && currentSpeedMultiplier != targetSpeedMultiplier)
            {
                if (speedTransitionDuration <= 0f)
                {
                    currentSpeedMultiplier = targetSpeedMultiplier;
                }
                else
                {
                    speedTransitionTime += Time.deltaTime;
                    float t = Mathf.Clamp01(speedTransitionTime / speedTransitionDuration);
                    currentSpeedMultiplier = Mathf.Lerp(startSpeedMultiplier, targetSpeedMultiplier, t);
                }
            }

            // Input buffering in Update
            if (inputController.ConsumeJumpRequest())
            {
                jumpRequestBuffered = true;
                jumpRequestTime = Time.unscaledTime;
                Debug.Log("[INPUT] Tap Buffered");
                
                // If it's a first jump (grounded), do it IMMEDIATELY here to guarantee 0-frame latency
                // As the user requested: "First Tap -> JUMP IMMEDIATELY"
                if (IsGrounded && hasLeftGroundSinceJump)
                {
                    ProcessFirstJump();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsAlive)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                return;
            }

            if (groundSensor != null) groundSensor.CheckGround();

            // Track genuine ground leaving
            if (!hasLeftGroundSinceJump && !IsGrounded)
            {
                hasLeftGroundSinceJump = true;
            }

            // Reset jump count on genuine landing
            if (hasLeftGroundSinceJump && IsGrounded && rb.velocity.y <= 0f)
            {
                jumpCount = 0;
                firstJumpTime = -1f;
            }

            HandleMovement();
            ProcessBufferedJumpPhysics();
            ApplyCustomGravity();
        }

        private void HandleMovement()
        {
            float activeSpeed = baseMoveSpeed;
            if (enableLevelSpeedProgression) activeSpeed *= currentSpeedMultiplier;

            rb.velocity = new Vector2(activeSpeed, rb.velocity.y);
        }

        private void ProcessFirstJump()
        {
            Debug.Log("[JUMP] First Jump Accepted Immediately");
            PerformJump(1.0f);
            jumpCount = 1;
            firstJumpTime = Time.unscaledTime;
            hasLeftGroundSinceJump = false;
            
            // Consume the buffer since we just used it
            jumpRequestBuffered = false;
        }

        private void ProcessBufferedJumpPhysics()
        {
            if (!jumpRequestBuffered) return;

            // Check if buffer expired
            if (Time.unscaledTime - jumpRequestTime > secondJumpInputBuffer)
            {
                jumpRequestBuffered = false;
                Debug.Log("[DOUBLE] Rejected - Buffer Expired");
                return;
            }

            // If grounded, perform first jump from buffer
            if (IsGrounded && hasLeftGroundSinceJump && jumpCount == 0)
            {
                ProcessFirstJump();
                return;
            }

            // Process SECOND jump
            if (jumpCount == 1)
            {
                if (!IsGrounded && hasLeftGroundSinceJump)
                {
                    if (Time.unscaledTime - firstJumpTime <= doubleTapWindow)
                    {
                        Debug.Log("[DOUBLE] Second Jump Accepted (Double Tap)");
                        PerformJump(secondJumpForceMultiplier);
                        jumpCount = 2;
                        jumpRequestBuffered = false; // consume
                    }
                    else
                    {
                        Debug.Log("[DOUBLE] Rejected - Window Expired");
                        jumpRequestBuffered = false; // consume
                    }
                }
            }
            else if (jumpCount > 1)
            {
                Debug.Log("[DOUBLE] Rejected - Max Jumps Reached");
                jumpRequestBuffered = false; // consume
            }
        }

        private void PerformJump(float forceMultiplier)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity * forceMultiplier);
            if (groundSensor != null) groundSensor.ForceUnground();
            OnJumpStarted?.Invoke();
        }

        private void ApplyCustomGravity()
        {
            if (rb.velocity.y < maxFallSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
            }
        }
    }
}
