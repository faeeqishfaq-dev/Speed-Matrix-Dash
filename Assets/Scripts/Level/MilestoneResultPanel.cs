using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace GeometryDash.Level
{
    public class MilestoneResultPanel : MonoBehaviour
    {
        [Header("Scene References")]
        public RectTransform panelRoot;
        public Button replayBtn;
        public Button nextBtn;
        public Button menuBtn;
        public TextMeshProUGUI attemptsText;

        private MilestoneCompletionDirector director;

        public void ShowPanel(MilestoneCompletionDirector dir)
        {
            director = dir;

            replayBtn.onClick.AddListener(() => { DisableButtons(); director.OnReplayClicked(); });
            nextBtn.onClick.AddListener(() => { DisableButtons(); director.OnNextClicked(); });
            menuBtn.onClick.AddListener(() => { DisableButtons(); director.OnMainMenuClicked(); });

            if (attemptsText != null)
            {
                LevelSequenceManager lsm = FindObjectOfType<LevelSequenceManager>();
                if (lsm != null)
                {
                    // Access current level ID
                    int currentLevelNumber = GeometryDash.Level.LevelSequenceManager.savedLevelIndex + 1;
                    int attempts = PlayerPrefs.GetInt($"LevelAttempt_{currentLevelNumber}", 1);
                    attemptsText.text = $"ATTEMPTS: {attempts}";
                }
            }

            StartCoroutine(SlideInRoutine());
        }

        private void DisableButtons()
        {
            replayBtn.interactable = false;
            nextBtn.interactable = false;
            menuBtn.interactable = false;
        }

        private IEnumerator SlideInRoutine()
        {
            float dur = director.PanelSlideDuration;
            float elapsed = 0;
            float startY = director.PanelStartOffset;
            float endY = 0f;

            panelRoot.anchoredPosition = new Vector2(0, startY);

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                // Back overshoot easing
                float s = 1.70158f;
                float p2 = p - 1;
                float ease = p2 * p2 * ((s + 1) * p2 + s) + 1;

                panelRoot.anchoredPosition = new Vector2(0, Mathf.LerpUnclamped(startY, endY, ease));
                yield return null;
            }

            panelRoot.anchoredPosition = new Vector2(0, endY);
        }
    }
}
