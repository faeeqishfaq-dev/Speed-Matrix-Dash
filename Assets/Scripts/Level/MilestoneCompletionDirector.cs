using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace GeometryDash.Level
{
    public class MilestoneCompletionDirector : MonoBehaviour
    {
        public static MilestoneCompletionDirector Instance { get; private set; }

        [Header("MILESTONE SETTINGS")]
        public List<int> MilestoneLevels = new List<int> { 4, 8, 12 };
        public float FinishWallOffset = 5f;
        public float CameraWallEdgeOffset = 0f;

        [Header("IDLE TUNING")]
        public float idleGlowIntensity = 1.0f;
        public float idleGlowPulseSpeed = 2.0f;
        public float idleParticleRate = 12f;

        [Header("CONTACT TUNING")]
        public float contactWaveDuration = 0.4f;
        public float contactWaveScale = 32f;
        public float contactFlashIntensity = 1.8f;

        [Header("BEAMS TUNING")]
        public float beamStartDelay = 0.25f;
        public float beamDuration = 4.2f;
        public float beamIntensity = 1.0f;
        public int beamCount = 16;
        public float beamSpreadAngle = 160f;
        public float BeamLength = 35f;
        public float BeamWidth = 2.5f;
        public Color BeamColor = new Color(1f, 0.85f, 0.1f, 0.9f);

        [Header("PARTICLES TUNING")]
        public int blastParticleCount = 85;
        public float blastParticleSpeed = 16f;
        public float blastParticleLifetime = 3.5f;
        public float secondaryParticleRate = 14f;

        [Header("SEQUENCE & PANEL TIMING")]
        public float totalCompletionSequenceDuration = 5.0f;
        public float panelShowDelay = 5.0f;

        [Header("LEVEL COMPLETE TEXT")]
        public float TextDelay = 1.8f;
        public float TextPopDuration = 0.35f;
        public float TextHoldDuration = 2.8f;

        [Header("PANEL SETTINGS")]
        public float PanelSlideDuration = 0.6f;
        public float PanelOvershoot = 1.1f;
        public float PanelStartOffset = 1500f; // from top

        [Header("REFERENCES")]
        public MilestoneCompletionVFX vfxPrefab;
        public MilestoneResultPanel panelPrefab;

        private MilestoneResultPanel activePanel;
        private MilestoneCompletionVFX activeVFX;
        private float originalTimeScale = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsMilestone(int level)
        {
            return MilestoneLevels.Contains(level);
        }

        public void BeginMilestone(Vector3 wallPosition)
        {
            StartCoroutine(CelebrationRoutine(wallPosition));
        }

        private IEnumerator CelebrationRoutine(Vector3 wallPos)
        {
            // 1. Lock Player (Freeze Time)
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // Camera is ALREADY CLAMPED by MilestoneCameraClamp, so we do NOT move it here!

            // 3. Play VFX
            if (vfxPrefab != null)
            {
                activeVFX = Instantiate(vfxPrefab, wallPos, Quaternion.identity);
                activeVFX.PlaySequence(this);
            }

            // 4. Wait for EXACTLY panelShowDelay (5.0 seconds) before showing panel
            yield return new WaitForSecondsRealtime(panelShowDelay);

            // 5. Show Panel
            if (panelPrefab != null)
            {
                activePanel = Instantiate(panelPrefab);
                activePanel.ShowPanel(this);
            }
        }

        public void OnReplayClicked()
        {
            CleanUp();
            Time.timeScale = originalTimeScale;
            CheckCpAd.instance.CallAd();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnNextClicked()
        {
            CleanUp();
            Time.timeScale = originalTimeScale;
            
            // Advance the level globally and restart the scene so ONLY the new level exists at X=0
            LevelSequenceManager.savedLevelIndex++;
            CheckCpAd.instance.CallAd();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void OnMainMenuClicked()
        {
            CleanUp();
            Time.timeScale = originalTimeScale;
            SceneManager.LoadScene("MainMenuScene");
        }

        private void CleanUp()
        {
            if (activeVFX != null) Destroy(activeVFX.gameObject);
            if (activePanel != null) Destroy(activePanel.gameObject);
            
            MilestoneCameraClamp clamp = UnityEngine.Camera.main.GetComponent<MilestoneCameraClamp>();
            if (clamp != null) clamp.DisableClamp();
        }
    }
}
