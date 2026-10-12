using UnityEngine;
using System.Collections;

namespace GeometryDash.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScaleAnimator : MonoBehaviour
    {
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private Vector3 hiddenScale = new Vector3(0.8f, 0.8f, 1f);
        [SerializeField] private Vector3 visibleScale = Vector3.one;

        private CanvasGroup canvasGroup;
        private Coroutine currentAnim;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(Animate(1f, visibleScale));
        }

        public void Hide()
        {
            if (!gameObject.activeInHierarchy) return;
            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(Animate(0f, hiddenScale, true));
        }

        private IEnumerator Animate(float targetAlpha, Vector3 targetScale, bool disableOnComplete = false)
        {
            // Use unscaled time so animations run even when Time.timeScale is 0 (e.g. paused)
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = transform.localScale;
            float t = 0;

            while (t < animationDuration)
            {
                t += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(t / animationDuration);
                
                // Ease out cubic
                float ease = 1f - Mathf.Pow(1f - normalizedTime, 3f);

                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, ease);
                transform.localScale = Vector3.Lerp(startScale, targetScale, ease);
                
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            transform.localScale = targetScale;

            if (disableOnComplete)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
