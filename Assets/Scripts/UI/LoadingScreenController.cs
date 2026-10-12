using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GeometryDash.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private Slider loadingBar;
        [SerializeField] private Text tipText;
        [SerializeField] private float loadingDuration = 2.5f;
        [SerializeField] private string nextSceneName = "MainMenuScene";

        private string[] tips = new string[]
        {
            "TAP TO JUMP",
            "TIME YOUR JUMPS CAREFULLY",
            "WATCH THE PLATFORM EDGES",
            "STAY FOCUSED",
            "SQUARE PARTICLES ARE COOL"
        };

        private void Start()
        {
            if (tipText != null)
            {
                tipText.text = tips[Random.Range(0, tips.Length)];
            }

            if (loadingBar != null)
            {
                loadingBar.value = 0f;
            }

            StartCoroutine(LoadSequence());
            //CheckCpAd.instance.CallAd();
        }

        private IEnumerator LoadSequence()
        {
            //CheckCpAd.instance.CallAd();
            float elapsed = 0f;

            // Optional: Start async loading early but don't activate

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
            if (asyncLoad != null)
            {
                asyncLoad.allowSceneActivation = false;
            }

            while (elapsed < loadingDuration)
            {
                elapsed += Time.deltaTime;
                if (loadingBar != null)
                {
                    loadingBar.value = Mathf.Clamp01(elapsed / loadingDuration);
                }
                yield return null;
            }

            if (loadingBar != null) loadingBar.value = 1f;

            if (asyncLoad != null)
            {
                asyncLoad.allowSceneActivation = true;
                CheckCpAd.instance.CallAd();

            }
            else
            {
                //CheckCpAd.instance.CallAd();
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
