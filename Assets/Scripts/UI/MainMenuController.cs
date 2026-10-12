using UnityEngine;
using UnityEngine.SceneManagement;
using GeometryDash.Level;

namespace GeometryDash.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIScaleAnimator settingsPanelAnimator;
        [SerializeField] private UIScaleAnimator levelSelectPanelAnimator;
        [SerializeField] private UIScaleAnimator skinSelectPanelAnimator;
        [SerializeField] private string playSceneName = "TestScene";

        public void OnPlayClicked()
        {
            // Main Menu Play button always begins at Level 1 as required
            LevelSequenceManager.savedLevelIndex = 0;
            CheckCpAd.instance.CallAd();
            SceneManager.LoadScene(playSceneName);
        }

        public void OnSettingsClicked()
        {
            if (levelSelectPanelAnimator != null) levelSelectPanelAnimator.Hide();
            if (skinSelectPanelAnimator != null) skinSelectPanelAnimator.Hide();
            if (settingsPanelAnimator != null)
            {
                settingsPanelAnimator.Show();
            }
        }

        public void OnSettingsClosed()
        {
            if (settingsPanelAnimator != null)
            {
                settingsPanelAnimator.Hide();
            }
        }

        public void OnLevelsClicked()
        {
            if (settingsPanelAnimator != null) settingsPanelAnimator.Hide();
            if (skinSelectPanelAnimator != null) skinSelectPanelAnimator.Hide();
            if (levelSelectPanelAnimator != null)
            {
                levelSelectPanelAnimator.Show();
            }
        }

        public void OnLevelsClosed()
        {
            if (levelSelectPanelAnimator != null)
            {
                levelSelectPanelAnimator.Hide();
            }
        }

        public void OnSkinsClicked()
        {
            if (settingsPanelAnimator != null) settingsPanelAnimator.Hide();
            if (levelSelectPanelAnimator != null) levelSelectPanelAnimator.Hide();
            if (skinSelectPanelAnimator != null)
            {
                skinSelectPanelAnimator.Show();
            }
        }

        public void OnSkinsClosed()
        {
            if (skinSelectPanelAnimator != null)
            {
                skinSelectPanelAnimator.Hide();
            }
        }
    }
}

