using UnityEngine;
using UnityEngine.UI;

namespace GeometryDash.UI
{
    public class AudioSettingsController : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private const string PREF_MUSIC = "MusicVolume";
        private const string PREF_SFX = "SFXVolume";

        private void OnEnable()
        {
            if (musicSlider != null)
            {
                musicSlider.value = PlayerPrefs.GetFloat(PREF_MUSIC, 1f);
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = PlayerPrefs.GetFloat(PREF_SFX, 1f);
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            }
        }

        private void OnMusicChanged(float value)
        {
            PlayerPrefs.SetFloat(PREF_MUSIC, value);
            PlayerPrefs.Save();
            // TODO: Hook into actual audio mixer if applicable
        }

        private void OnSFXChanged(float value)
        {
            PlayerPrefs.SetFloat(PREF_SFX, value);
            PlayerPrefs.Save();
            // TODO: Hook into actual audio mixer if applicable
        }
    }
}
