using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace GeometryDash.Level
{
    public class MilestoneLevelCompleteController : MonoBehaviour
    {
        [Header("MILESTONE LEVEL COMPLETE")]
        public bool EnableFeature = true;
        public List<int> MilestoneLevels = new List<int> { 4, 8, 12 };
        public float PresentationDuration = 3.0f;

        [Header("TEXT")]
        public string LevelCompleteText = "LEVEL COMPLETE!";
        public int FontSize = 120;
        public Color TextColor = new Color(0.6f, 1f, 0.2f);
        public float OutlineWidth = 0.2f;
        public float PopDuration = 0.25f;

        [Header("GOLDEN BLAST")]
        public Color BlastColor = new Color(1f, 0.8f, 0f, 0.8f);
        public float BlastDuration = 0.8f;
        public float BlastMaxScale = 25f;

        [Header("RAYS")]
        public int RayCount = 12;
        public float RayMinLength = 10f;
        public float RayMaxLength = 30f;
        public float RayMinWidth = 0.5f;
        public float RayMaxWidth = 2f;
        public float RayDuration = 0.8f;
        public float RayFadeDuration = 0.6f;

        [Header("PIXELS")]
        public int ParticleCount = 60;
        public float ParticleLifetime = 1.5f;
        public float ParticleSpeed = 15f;
        public float ParticleSize = 0.4f;

        [Header("FLOATING ORBS")]
        public int OrbCount = 8;
        public float OrbMinSize = 2f;
        public float OrbMaxSize = 6f;
        public float OrbLifetime = 2.0f;
        public float OrbDriftSpeed = 2f;

        [Header("REFERENCES")]
        public GameObject VFXPrefab;
        
        public static MilestoneLevelCompleteController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsMilestoneLevel(int levelNumber)
        {
            if (!EnableFeature) return false;
            return MilestoneLevels.Contains(levelNumber);
        }

        public void PlayMilestoneCompletion(Vector3 completionOrigin, System.Action onComplete)
        {
            StartCoroutine(CelebrationRoutine(completionOrigin, onComplete));
        }

        private IEnumerator CelebrationRoutine(Vector3 origin, System.Action onComplete)
        {
            float previousTimeScale = Time.timeScale;
            Time.timeScale = 0f; // Pause gameplay safely

            if (VFXPrefab != null)
            {
                GameObject vfx = Instantiate(VFXPrefab, origin, Quaternion.identity);
                LevelCompleteVFXController vfxController = vfx.GetComponent<LevelCompleteVFXController>();
                if (vfxController != null)
                {
                    vfxController.Initialize(this, origin);
                }
                
                // Wait using unscaled time
                yield return new WaitForSecondsRealtime(PresentationDuration);
                
                Destroy(vfx);
            }
            else
            {
                yield return new WaitForSecondsRealtime(PresentationDuration);
            }

            Time.timeScale = previousTimeScale;
            onComplete?.Invoke();
        }
    }
}
