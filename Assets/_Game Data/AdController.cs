using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AdController : MonoBehaviour
{
    [Header("Ad Settings")]
    public float adInterval = 30f; // Time between ads (in seconds)

    [Header("UI References")]
    public GameObject adCountdownPanel;
    public Text countdownText;
    //public GameObject AdsPanel;
    private bool isCountingDown = false;

    void Start()
    {
        adCountdownPanel.SetActive(false);
        StartCoroutine(AdCycle());
    }

    IEnumerator AdCycle()
    {
        while (true)
        {
            // Wait for the ad interval before showing the countdown
            yield return new WaitForSeconds(adInterval);
            yield return StartCoroutine(ShowCountdown());
            ShowAdPanel();
        }
    }

    IEnumerator ShowCountdown()
    {
        isCountingDown = true;
        adCountdownPanel.SetActive(true);

        int countdownTime = 10;



        while (countdownTime > 0)
        {

            if (PlayerPrefs.GetInt("RemoveAds", 0) == 0)
            {

                countdownText.text = $"00:{countdownTime:00}";
                yield return new WaitForSeconds(1f);
                countdownTime--;
            }
        }

        adCountdownPanel.SetActive(false);
        isCountingDown = false;
    }

    void ShowAdPanel()
    {
        CheckCpAd.instance.CallAd();
        //MediationHandler.Instance.ShowMediumBanner();
        //AdsPanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

   public void ShowAd()
    {
        CheckCpAd.instance.CallAd();
        Time.timeScale = 1f;
        //MediationHandler.Instance.ShowInterstitial();
        //MediationHandler.Instance.HideMediumBanner();
        //AdsPanel.gameObject.SetActive(false);
        Invoke(nameof(HideAdPanel), 0.5f);

    }

    public void RemoveAdsHandler()
    {
        PlayerPrefs.SetInt("RemoveAds", 1);
        Time.timeScale = 1f;
        //MediationHandler.Instance.HideMediumBanner();
        //MediationHandler.Instance.HideSmallBanner();
        //MediationHandler.Instance.HideSmallBanner2();
        Invoke(nameof(HideAdPanel),0.5f);

    }

    public void HideAdPanel()
    {
        //AdsPanel.gameObject.SetActive(false);
     

    }
   

}
