using UnityEngine;

namespace GeometryDash.Player
{
    //[DefaultExecutionOrder(-100)]
    public class PlayerInputController : MonoBehaviour
    {
        private bool jumpRequested;
        
        public bool InputEnabled { get; set; } = true;

        private void Update()
        {
            if (!InputEnabled)
            {
                jumpRequested = false;
                return;
            }

            bool jumpDownThisFrame = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("[INPUT] New Input System - Space Pressed");
                jumpDownThisFrame = true;
            }
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log("[INPUT] New Input System - Mouse Pressed");
                jumpDownThisFrame = true;
            }
            if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Debug.Log("[INPUT] New Input System - Touch Began");
                jumpDownThisFrame = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[INPUT] Legacy Input - Space Pressed");
                jumpDownThisFrame = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("[INPUT] Legacy Input - Mouse Pressed");
                jumpDownThisFrame = true;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    Debug.Log("[INPUT] Legacy Input - Touch Began");
                    jumpDownThisFrame = true;
                }
            }
#endif

            if (jumpDownThisFrame)
            {
                Debug.Log("[INPUT] Jump Request Sent to Buffer");
                jumpRequested = true;
            }
        }

        public bool ConsumeJumpRequest()
        {
            if (jumpRequested)
            {
                jumpRequested = false;
                return true;
            }
            return false;
        }
    }
}
