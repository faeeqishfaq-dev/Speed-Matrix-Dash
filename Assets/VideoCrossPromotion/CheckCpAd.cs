using System;
using UnityEngine;

public class CheckCpAd : MonoBehaviour
{
    public static CheckCpAd instance;
    private int adReadyTime = 30; 

    private float timer = 0f;
    private bool isAdReady = false;

    public GameObject cpVideoAd; 
    public bool IsAdReady => isAdReady;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
       
        instance = this;
        adReadyTime = 1;

        timer = 0f;
        Debug.Log("AdReadyTime from RemoteConfigManager:" + adReadyTime + "seconds");
    }

    void Update()
    {
        if (!isAdReady)
        {
            timer += Time.deltaTime;

            if (timer >= adReadyTime)
            {
                isAdReady = true;
                Debug.Log("Ad is ready");
                
            }
        }
    }

    public void CallAd()
    {
        if (isAdReady)
        {
            Debug.Log("Ad called");
            cpVideoAd.SetActive(true);
            isAdReady = false;
            timer = 0f;
        }
        else
        {
            Debug.Log("Ad not ready yet.");
        }
    }
}
