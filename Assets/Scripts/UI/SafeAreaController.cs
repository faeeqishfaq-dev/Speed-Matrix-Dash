using UnityEngine;

namespace GeometryDash.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaController : MonoBehaviour
    {
        private RectTransform panel;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);

        void Awake()
        {
            panel = GetComponent<RectTransform>();
            Refresh();
        }

        void Update()
        {
            if (lastSafeArea != Screen.safeArea)
            {
                Refresh();
            }
        }

        void Refresh()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }
    }
}
