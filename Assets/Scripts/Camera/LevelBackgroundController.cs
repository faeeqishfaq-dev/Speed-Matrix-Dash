using UnityEngine;
using GeometryDash.Level;

namespace GeometryDash.Camera
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class LevelBackgroundController : MonoBehaviour
    {
        public enum FitMode
        {
            Cover,
            Contain
        }

        [Header("REFERENCES")]
        [Tooltip("The camera to fit the background to. Defaults to Main Camera if unassigned.")]
        public UnityEngine.Camera targetCamera;

        [Tooltip("The SpriteRenderer used to render the full-screen background.")]
        public SpriteRenderer backgroundRenderer;

        [Header("FIT SETTINGS")]
        [Tooltip("Cover ensures full-screen coverage with zero black bars while preserving original aspect ratio.")]
        public FitMode fitMode = FitMode.Cover;

        [Tooltip("Local Z distance relative to camera container (places background behind gameplay at Z=0).")]
        public float localZDistance = 15f;

        [Tooltip("Slight buffer factor (e.g. 0.5%) to eliminate subpixel seam gaps at screen borders.")]
        public float edgeBuffer = 1.005f;

        [Header("DEFAULT / FALLBACK SPRITES")]
        public Sprite blueBackground;
        public Sprite nightBackground;
        public Sprite fireBackground;

        // Cached values to avoid redundant calculations
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private float lastCamSize = -1f;
        private float lastCamFov = -1f;
        private float lastCamAspect = -1f;
        private Sprite lastSprite = null;

        private void Awake()
        {
            InitializeReferences();
        }

        private void OnEnable()
        {
            InitializeReferences();
            LevelSequenceManager.OnLevelAttemptStarted += HandleLevelStarted;
            RecalculateBackgroundFit(true);
        }

        private void OnDisable()
        {
            LevelSequenceManager.OnLevelAttemptStarted -= HandleLevelStarted;
        }

        private void Start()
        {
            InitializeReferences();
            
            // Initial check: if running in Play Mode, try to sync with current active level
            if (Application.isPlaying)
            {
                int currentLvl = LevelSequenceManager.savedLevelIndex + 1;
                if (backgroundRenderer != null && backgroundRenderer.sprite == null)
                {
                    Sprite initialSprite = GetFallbackSprite(currentLvl);
                    SetBackgroundSprite(initialSprite);
                }
            }

            RecalculateBackgroundFit(true);
        }

        private void LateUpdate()
        {
            // Keep local position centered with constant Z depth
            transform.localPosition = new Vector3(0f, 0f, localZDistance);
            transform.localRotation = Quaternion.identity;

            // Recalculate only if screen resolution, camera parameters, or sprite changed
            if (targetCamera == null) targetCamera = UnityEngine.Camera.main;
            if (targetCamera == null || backgroundRenderer == null) return;

            if (Screen.width != lastScreenWidth ||
                Screen.height != lastScreenHeight ||
                !Mathf.Approximately(targetCamera.orthographicSize, lastCamSize) ||
                !Mathf.Approximately(targetCamera.fieldOfView, lastCamFov) ||
                !Mathf.Approximately(targetCamera.aspect, lastCamAspect) ||
                backgroundRenderer.sprite != lastSprite)
            {
                RecalculateBackgroundFit();
            }
        }

        private void OnValidate()
        {
            InitializeReferences();
            RecalculateBackgroundFit(true);
        }

        public void InitializeReferences()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponentInParent<UnityEngine.Camera>();
                if (targetCamera == null) targetCamera = UnityEngine.Camera.main;
            }

            if (backgroundRenderer == null)
            {
                Transform child = transform.Find("BackgroundSprite");
                if (child != null)
                {
                    backgroundRenderer = child.GetComponent<SpriteRenderer>();
                }
                else
                {
                    backgroundRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }

            // Ensure correct sorting order
            if (backgroundRenderer != null)
            {
                backgroundRenderer.sortingLayerName = "Default";
                if (backgroundRenderer.sortingOrder >= 0)
                {
                    backgroundRenderer.sortingOrder = -100;
                }
            }
        }

        private void HandleLevelStarted(LevelDefinition levelDef)
        {
            if (levelDef == null) return;

            Sprite chosen = levelDef.BackgroundSprite;
            if (chosen == null)
            {
                chosen = GetFallbackSprite(levelDef.LevelIndex);
            }

            SetBackgroundSprite(chosen);
        }

        public void SetBackgroundSprite(Sprite newSprite)
        {
            if (backgroundRenderer == null || newSprite == null) return;

            backgroundRenderer.sprite = newSprite;
            RecalculateBackgroundFit(true);
        }

        public Sprite GetFallbackSprite(int levelIndex)
        {
            switch (levelIndex)
            {
                case 1:  return fireBackground;
                case 2:  return blueBackground;
                case 3:  return fireBackground;
                case 4:  return nightBackground;
                case 5:  return fireBackground;
                case 6:  return blueBackground;
                case 7:  return fireBackground;
                case 8:  return nightBackground;
                case 9:  return fireBackground;
                case 10: return blueBackground;
                case 11: return fireBackground;
                case 12: return nightBackground;
                case 13: return fireBackground;
                case 14: return blueBackground;
                case 15: return fireBackground;
                default:
                    if (levelIndex % 2 != 0) return fireBackground;
                    if (levelIndex % 4 == 2) return blueBackground;
                    return nightBackground;
            }
        }

        public void RecalculateBackgroundFit(bool force = false)
        {
            if (targetCamera == null) targetCamera = UnityEngine.Camera.main;
            if (targetCamera == null || backgroundRenderer == null || backgroundRenderer.sprite == null) return;

            Sprite sprite = backgroundRenderer.sprite;
            float spriteWidth = sprite.bounds.size.x;
            float spriteHeight = sprite.bounds.size.y;

            if (spriteWidth <= 0f || spriteHeight <= 0f) return;

            // 1. Calculate camera visible world bounds
            float aspect = targetCamera.aspect;
            if (aspect <= 0.001f)
            {
                aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            }

            float visibleWorldHeight;
            float visibleWorldWidth;

            if (targetCamera.orthographic)
            {
                visibleWorldHeight = targetCamera.orthographicSize * 2f;
                visibleWorldWidth = visibleWorldHeight * aspect;
            }
            else
            {
                // Perspective camera: calculate visible view frustum at background Z distance along camera forward
                float distance = Vector3.Dot(transform.position - targetCamera.transform.position, targetCamera.transform.forward);
                if (distance <= 0.1f) distance = Mathf.Abs(localZDistance);
                visibleWorldHeight = 2.0f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                visibleWorldWidth = visibleWorldHeight * aspect;
            }

            // 2. Compute Cover / Contain scaling factors
            float scaleX = visibleWorldWidth / spriteWidth;
            float scaleY = visibleWorldHeight / spriteHeight;

            float finalScale;
            if (fitMode == FitMode.Cover)
            {
                // Max ensures full coverage without black bars
                finalScale = Mathf.Max(scaleX, scaleY) * edgeBuffer;
            }
            else
            {
                // Min ensures complete sprite fits without cropping
                finalScale = Mathf.Min(scaleX, scaleY);
            }

            // 3. Apply uniform scale to maintain original aspect ratio (no stretching or squashing)
            backgroundRenderer.transform.localScale = new Vector3(finalScale, finalScale, 1f);
            backgroundRenderer.transform.localPosition = Vector3.zero;

            // Cache current settings
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastCamSize = targetCamera.orthographicSize;
            lastCamFov = targetCamera.fieldOfView;
            lastCamAspect = targetCamera.aspect;
            lastSprite = sprite;
        }
    }
}
