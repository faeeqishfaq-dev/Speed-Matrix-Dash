using UnityEngine;

namespace GeometryDash.UI
{
    /// <summary>
    /// Handles exiting the application when the Quit button is pressed.
    /// Exits Play Mode when run within the Unity Editor and calls Application.Quit() in standalone/mobile builds.
    /// </summary>
    public class ApplicationQuitController : MonoBehaviour
    {
        public void QuitGame()
        {
            Debug.Log("[ApplicationQuitController] Quit button clicked. Exiting application.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
