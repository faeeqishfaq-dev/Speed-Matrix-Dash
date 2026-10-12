using UnityEngine;

namespace GeometryDash.UI
{
    /// <summary>
    /// Adds a subtle continuous vertical floating/hovering animation to the Main Menu Play Button.
    /// Operates on RectTransform.anchoredPosition using unscaled time to avoid position drift.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class MainMenuPlayButtonFloat : MonoBehaviour
    {
        [Header("Float Settings")]
        [Tooltip("Enable or disable continuous floating motion.")]
        [SerializeField] private bool enableFloat = true;

        [Tooltip("Maximum vertical float distance in UI pixels (8 to 15 recommended).")]
        [SerializeField] private float floatDistance = 15f;

        [Tooltip("Speed multiplier for floating oscillation (1.5 to 2.0 recommended).")]
        [SerializeField] private float floatSpeed = 5f;

        private RectTransform _rectTransform;
        private Vector2 _startPosition;
        private bool _isInitialized = false;

        public bool EnableFloat
        {
            get => enableFloat;
            set => enableFloat = value;
        }

        public float FloatDistance
        {
            get => floatDistance;
            set => floatDistance = value;
        }

        public float FloatSpeed
        {
            get => floatSpeed;
            set => floatSpeed = value;
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _startPosition = _rectTransform.anchoredPosition;
                _isInitialized = true;
            }
        }

        private void Update()
        {
            if (!enableFloat || _rectTransform == null) return;

            // Pure absolute offset from cached original start position using unscaled time
            float offsetY = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatDistance;
            _rectTransform.anchoredPosition = _startPosition + new Vector2(0f, offsetY);
        }

        private void OnDisable()
        {
            // Restore starting position cleanly when disabled
            if (_rectTransform != null && _isInitialized)
            {
                _rectTransform.anchoredPosition = _startPosition;
            }
        }
    }
}
