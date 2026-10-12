using UnityEngine;
using UnityEngine.SceneManagement;

namespace GeometryDash.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private UIScaleAnimator pauseAnimator;
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        private bool isPaused = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    OnResumeClicked();
                }
                else
                {
                    OnPauseClicked();
                }
            }
        }

        public void OnPauseClicked()
        {
            if (isPaused) return;

            isPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null) pausePanel.SetActive(true);
            if (pauseAnimator != null) pauseAnimator.Show();
        }

        public void OnResumeClicked()
        {
            CheckCpAd.instance.CallAd();

            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;

            if (pauseAnimator != null)
            {
                pauseAnimator.Hide();
            }
            else if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            CheckCpAd.instance.CallAd();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            CheckCpAd.instance.CallAd();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void OnPracticeClicked()
        {
            Debug.Log("[Pause] Practice mode clicked. (Not implemented in this phase)");
        }
    }
}
