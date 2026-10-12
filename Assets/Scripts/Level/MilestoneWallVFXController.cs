using UnityEngine;
using System.Collections;

namespace GeometryDash.Level
{
    public class MilestoneWallVFXController : MonoBehaviour
    {
        [Header("FINISH WALL VFX REFERENCES")]
        public GameObject wallEnergyGlow;
        public ParticleSystem wallParticleEmitter;

        [Header("Glow Renderers")]
        public SpriteRenderer coreGlow;
        public SpriteRenderer midGlow;
        public SpriteRenderer outerGlow;
        public SpriteRenderer purpleWallGlow;

        [Header("Idle Tuning")]
        public float idleGlowIntensity = 1.0f;
        public float idleGlowPulseSpeed = 2.0f;
        public float idleParticleRate = 12f;

        [Header("Glow Settings (Core)")]
        public Color coreColor = new Color(1f, 0.98f, 0.82f, 1f); // #FFFBD0
        public float coreWidth = 0.4f;
        public float coreBaseAlpha = 0.65f;
        public float coreChargeAlpha = 1.0f;

        [Header("Glow Settings (Mid)")]
        public Color midColor = new Color(1f, 0.84f, 0.16f, 1f); // #FFD52A
        public float midWidth = 2.0f;
        public float midBaseAlpha = 0.35f;
        public float midChargeAlpha = 0.80f;

        [Header("Glow Settings (Outer)")]
        public Color outerColor = new Color(1f, 0.6f, 0.13f, 1f); // #FF9A20
        public float outerWidth = 5.0f;
        public float outerBaseAlpha = 0.20f;
        public float outerChargeAlpha = 0.55f;

        [Header("Glow Settings (Purple Wall Aura)")]
        public Color purpleColor = new Color(0.75f, 0.15f, 1f, 0.35f); // #C020FF
        public float purpleWidth = 3.5f;
        public float purpleBaseAlpha = 0.30f;
        public float purpleChargeAlpha = 0.60f;

        [Header("Animation Timings")]
        public float contactFlashDuration = 0.15f;
        public float totalCelebrationDuration = 5.0f;
        
        [Header("Particles")]
        public ParticleSystem mainParticles;
        public ParticleSystem microParticles;

        [Header("LIGHT BEAMS")]
        public GameObject lightBeamEmitter;
        public SpriteRenderer[] beamRenderers;
        public float beamStartDelay = 0.25f;
        public float beamExpandDuration = 0.35f;
        public float beamSustainDuration = 3.0f;
        public float beamFadeDuration = 1.0f;

        [Header("Beam Stagger Timings")]
        public float beamGroup2Delay = 0.12f;
        public float beamGroup3Delay = 0.24f;

        [Header("Beam Shake / Vibration")]
        public bool enableBeamShake = true;
        public float beamShakeStrength = 0.5f;
        public float beamShakeFrequency = 20f;

        private Vector3[] originalLocalPositions;
        private Quaternion[] originalLocalRotations;

        private readonly float[] defaultAngles = { 45f, 65f, 82f, 98f, 115f, 135f };
        private readonly float[] defaultWidths = { 0.40f, 0.48f, 0.52f, 0.50f, 0.45f, 0.38f };
        private readonly float[] defaultLengths = { 24f, 28f, 32f, 30f, 26f, 22f };
        private readonly Color[] defaultColors = {
            new Color(1.0f, 0.90f, 0.40f, 0.85f),
            new Color(1.0f, 0.98f, 0.75f, 0.95f),
            new Color(1.0f, 0.85f, 0.25f, 0.90f),
            new Color(1.0f, 0.92f, 0.60f, 0.90f),
            new Color(1.0f, 0.78f, 0.20f, 0.85f),
            new Color(1.0f, 0.88f, 0.35f, 0.80f)
        };

        private Coroutine animCoroutine;
        private Coroutine beamCoroutine;
        private bool isCompleted = false;

        private void Awake()
        {
            FindExistingReferences();
            ResetBeams();
        }

        private void OnEnable()
        {
            ResetVFX();
        }

        public void ResetVFX()
        {
            isCompleted = false;
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
                animCoroutine = null;
            }
            if (beamCoroutine != null)
            {
                StopCoroutine(beamCoroutine);
                beamCoroutine = null;
            }

            ResetBeams();
            
            SetGlow(coreBaseAlpha, midBaseAlpha, outerBaseAlpha, purpleBaseAlpha, 1f);
            
            if (mainParticles != null)
            {
                mainParticles.Clear();
                var em = mainParticles.emission;
                em.rateOverTime = idleParticleRate;
                mainParticles.Play();
            }
            if (microParticles != null)
            {
                microParticles.Clear();
                var em = microParticles.emission;
                em.rateOverTime = idleParticleRate * 1.5f;
                microParticles.Play();
            }
        }

        private void Update()
        {
            // Continuous Idle gentle breathing pulse (alive wall effect)
            if (!isCompleted)
            {
                float pulse = Mathf.Sin(Time.unscaledTime * idleGlowPulseSpeed) * 0.12f + 0.88f;
                SetGlow(
                    coreBaseAlpha * pulse * idleGlowIntensity,
                    midBaseAlpha * pulse * idleGlowIntensity,
                    outerBaseAlpha * pulse * idleGlowIntensity,
                    purpleBaseAlpha * pulse * idleGlowIntensity,
                    1f
                );
            }
        }

        public void TriggerContactFlash()
        {
            OnPlayerContact();
        }

        public void OnPlayerContact()
        {
            if (isCompleted) return;
            isCompleted = true;

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(CelebrationWallSequence());

            if (beamCoroutine != null) StopCoroutine(beamCoroutine);
            beamCoroutine = StartCoroutine(PlayPermanentBeamsRoutine());
        }

        private IEnumerator CelebrationWallSequence()
        {
            // 1. Instantaneous Impact peak flash (0.0 to 0.15s)
            SetGlow(1.5f, 1.0f, 0.8f, 0.9f, 1.25f);
            
            if (mainParticles != null)
            {
                mainParticles.Emit(40); // local contact burst
                var em = mainParticles.emission;
                em.rateOverTime = idleParticleRate * 0.5f; // keep floating particles active
            }

            float elapsed = 0;
            while (elapsed < contactFlashDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 2. High-energy charged celebration stage (0.15s to 3.8s)
            // Wall stays charged with golden/purple energy throughout the beam & particle celebration
            float chargedDuration = 3.65f;
            elapsed = 0;
            while (elapsed < chargedDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / chargedDuration;
                // Subtle energetic shimmer during celebration
                float shimmer = Mathf.Sin(elapsed * 8f) * 0.08f + 0.92f;
                SetGlow(
                    coreChargeAlpha * shimmer,
                    midChargeAlpha * shimmer,
                    outerChargeAlpha * shimmer,
                    purpleChargeAlpha * shimmer,
                    1.1f
                );
                yield return null;
            }

            // 3. Graceful Settle stage (3.8s to 5.0s)
            // Softly settles as the Level Complete panel prepares to appear
            float settleDuration = totalCelebrationDuration - contactFlashDuration - chargedDuration;
            if (settleDuration <= 0f) settleDuration = 1.2f;

            elapsed = 0;
            while (elapsed < settleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / settleDuration;
                
                SetGlow(
                    Mathf.Lerp(coreChargeAlpha, coreBaseAlpha * 0.8f, t),
                    Mathf.Lerp(midChargeAlpha, midBaseAlpha * 0.8f, t),
                    Mathf.Lerp(outerChargeAlpha, outerBaseAlpha * 0.8f, t),
                    Mathf.Lerp(purpleChargeAlpha, purpleBaseAlpha * 0.8f, t),
                    Mathf.Lerp(1.1f, 1.0f, t)
                );
                yield return null;
            }

            SetGlow(coreBaseAlpha * 0.8f, midBaseAlpha * 0.8f, outerBaseAlpha * 0.8f, purpleBaseAlpha * 0.8f, 1f);
        }

        private void SetGlow(float coreA, float midA, float outerA, float purpleA, float widthMult)
        {
            if (coreGlow != null)
            {
                Color c = coreColor; c.a = Mathf.Clamp01(coreA);
                coreGlow.color = c;
                coreGlow.transform.localScale = new Vector3(coreWidth * widthMult, 20f, 1f);
            }
            if (midGlow != null)
            {
                Color c = midColor; c.a = Mathf.Clamp01(midA);
                midGlow.color = c;
                midGlow.transform.localScale = new Vector3(midWidth * widthMult, 20f, 1f);
            }
            if (outerGlow != null)
            {
                Color c = outerColor; c.a = Mathf.Clamp01(outerA);
                outerGlow.color = c;
                outerGlow.transform.localScale = new Vector3(outerWidth * widthMult, 20f, 1f);
            }
            if (purpleWallGlow != null)
            {
                Color c = purpleColor; c.a = Mathf.Clamp01(purpleA);
                purpleWallGlow.color = c;
                purpleWallGlow.transform.localScale = new Vector3(purpleWidth * widthMult, 20f, 1f);
            }
        }

        public void FindExistingReferences()
        {
            // Strictly reference already-existing editor objects. NEVER create new GameObjects or Clones at runtime.
            Transform glowRoot = transform.Find("WallEnergyGlow");
            if (glowRoot != null)
            {
                if (wallEnergyGlow == null) wallEnergyGlow = glowRoot.gameObject;
                if (outerGlow == null) { Transform t = glowRoot.Find("OuterGlow"); if (t != null) outerGlow = t.GetComponent<SpriteRenderer>(); }
                if (purpleWallGlow == null) { Transform t = glowRoot.Find("PurpleWallGlow"); if (t != null) purpleWallGlow = t.GetComponent<SpriteRenderer>(); }
                if (midGlow == null) { Transform t = glowRoot.Find("MidGlow"); if (t != null) midGlow = t.GetComponent<SpriteRenderer>(); }
                if (coreGlow == null) { Transform t = glowRoot.Find("CoreGlow"); if (t != null) coreGlow = t.GetComponent<SpriteRenderer>(); }
            }

            Transform partRoot = transform.Find("WallParticleEmitter");
            if (partRoot != null)
            {
                if (mainParticles == null)
                {
                    Transform t = partRoot.Find("MainParticles");
                    if (t != null) mainParticles = t.GetComponent<ParticleSystem>();
                }
                if (wallParticleEmitter == null) wallParticleEmitter = mainParticles;
            }

            Transform beamRoot = transform.Find("LightBeamEmitter");
            if (beamRoot != null)
            {
                if (lightBeamEmitter == null) lightBeamEmitter = beamRoot.gameObject;
                if (beamRenderers == null || beamRenderers.Length != 6)
                {
                    beamRenderers = new SpriteRenderer[6];
                }
                for (int i = 0; i < 6; i++)
                {
                    Transform child = beamRoot.Find($"Beam_0{i + 1}");
                    if (child != null)
                    {
                        beamRenderers[i] = child.GetComponent<SpriteRenderer>();
                    }
                }
            }

            // Store original local transforms once for drift-free shake restoration
            if (beamRenderers != null && beamRenderers.Length > 0)
            {
                if (originalLocalPositions == null || originalLocalPositions.Length != beamRenderers.Length)
                {
                    originalLocalPositions = new Vector3[beamRenderers.Length];
                    originalLocalRotations = new Quaternion[beamRenderers.Length];
                }
                for (int i = 0; i < beamRenderers.Length; i++)
                {
                    if (beamRenderers[i] != null)
                    {
                        originalLocalPositions[i] = beamRenderers[i].transform.localPosition;
                        originalLocalRotations[i] = beamRenderers[i].transform.localRotation;
                    }
                }
            }
        }

        private float GetBeamStartOffset(int index)
        {
            if (beamRenderers != null && index >= 0 && index < beamRenderers.Length && beamRenderers[index] != null)
            {
                string bName = beamRenderers[index].gameObject.name;
                // Stage 1: Beam_05 + Beam_02
                if (bName.EndsWith("05") || bName.EndsWith("02")) return 0.0f;
                // Stage 2: Beam_06 + Beam_03 + Beam_01
                if (bName.EndsWith("06") || bName.EndsWith("03") || bName.EndsWith("01")) return beamGroup2Delay;
                // Stage 3: Beam_04
                if (bName.EndsWith("04")) return beamGroup3Delay;
            }

            switch (index)
            {
                case 4: // Beam_05
                case 1: // Beam_02
                    return 0.0f;
                case 5: // Beam_06
                case 2: // Beam_03
                case 0: // Beam_01
                    return beamGroup2Delay;
                case 3: // Beam_04
                    return beamGroup3Delay;
                default:
                    return 0.0f;
            }
        }

        private float GetShakeOffset(int beamIndex, float timeVal)
        {
            if (!enableBeamShake) return 0f;
            // Smooth multi-frequency harmonic vibration with golden ratio phase offset per beam
            float tVal = timeVal * beamShakeFrequency * Mathf.PI * 2f + beamIndex * 1.618f;
            return (Mathf.Sin(tVal) * 0.7f + Mathf.Sin(tVal * 1.414f) * 0.3f) * beamShakeStrength;
        }

        public void ResetBeams()
        {
            if (beamRenderers == null) return;

            for (int i = 0; i < beamRenderers.Length; i++)
            {
                if (beamRenderers[i] != null)
                {
                    beamRenderers[i].transform.localScale = Vector3.zero;
                    Color c = beamRenderers[i].color;
                    c.a = 0f;
                    beamRenderers[i].color = c;

                    // Restore exact original local transforms
                    if (originalLocalRotations != null && i < originalLocalRotations.Length && originalLocalRotations[i] != default(Quaternion))
                    {
                        beamRenderers[i].transform.localRotation = originalLocalRotations[i];
                    }
                    if (originalLocalPositions != null && i < originalLocalPositions.Length)
                    {
                        beamRenderers[i].transform.localPosition = originalLocalPositions[i];
                    }
                }
            }
        }

        private IEnumerator PlayPermanentBeamsRoutine()
        {
            yield return new WaitForSecondsRealtime(beamStartDelay);

            if (beamRenderers == null || beamRenderers.Length == 0) yield break;

            int count = beamRenderers.Length;
            Vector3[] targetScales = new Vector3[count];
            Color[] targetColors = new Color[count];

            for (int i = 0; i < count; i++)
            {
                if (beamRenderers[i] == null) continue;
                float w = (i < defaultWidths.Length) ? defaultWidths[i] : 0.45f;
                float l = (i < defaultLengths.Length) ? defaultLengths[i] : 28f;
                targetScales[i] = new Vector3(w, l, 1f);

                Color c = (i < defaultColors.Length) ? defaultColors[i] : new Color(1f, 0.88f, 0.35f, 0.85f);
                if (beamRenderers[i].color.r > 0f || beamRenderers[i].color.g > 0f || beamRenderers[i].color.b > 0f)
                {
                    c = beamRenderers[i].color;
                    if (c.a <= 0.05f) c.a = 0.85f;
                }
                targetColors[i] = c;
            }

            float fadeStartTime = beamGroup3Delay + beamExpandDuration + beamSustainDuration;
            float totalDuration = fadeStartTime + beamFadeDuration;

            float elapsed = 0f;
            while (elapsed < totalDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                for (int i = 0; i < count; i++)
                {
                    if (beamRenderers[i] == null) continue;

                    float startOffset = GetBeamStartOffset(i);

                    if (elapsed < startOffset)
                    {
                        // Beam has not activated yet
                        beamRenderers[i].transform.localScale = Vector3.zero;
                        Color c = targetColors[i];
                        c.a = 0f;
                        beamRenderers[i].color = c;
                        if (originalLocalRotations != null && i < originalLocalRotations.Length && originalLocalRotations[i] != default(Quaternion))
                        {
                            beamRenderers[i].transform.localRotation = originalLocalRotations[i];
                        }
                    }
                    else if (elapsed < startOffset + beamExpandDuration)
                    {
                        // Expanding In stage
                        float p = Mathf.Clamp01((elapsed - startOffset) / beamExpandDuration);
                        float easeOut = 1f - Mathf.Pow(1f - p, 3f);

                        beamRenderers[i].transform.localScale = new Vector3(
                            targetScales[i].x * easeOut,
                            targetScales[i].y * easeOut,
                            1f
                        );
                        Color c = targetColors[i];
                        c.a = targetColors[i].a * easeOut;
                        beamRenderers[i].color = c;

                        // Subtle shake begins with expansion
                        float rotOffset = GetShakeOffset(i, elapsed);
                        if (originalLocalRotations != null && i < originalLocalRotations.Length && originalLocalRotations[i] != default(Quaternion))
                        {
                            beamRenderers[i].transform.localRotation = originalLocalRotations[i] * Quaternion.Euler(0f, 0f, rotOffset * easeOut);
                        }
                    }
                    else if (elapsed < fadeStartTime)
                    {
                        // Sustained Celebration stage (shimmer + breathing pulse + energetic alive shake)
                        float shimmer = Mathf.Sin(elapsed * 4f) * 0.08f + 0.92f;
                        float pulse = Mathf.Sin(elapsed * 3f + i * 1.05f) * 0.06f + 0.94f;

                        beamRenderers[i].transform.localScale = new Vector3(
                            targetScales[i].x * pulse,
                            targetScales[i].y,
                            1f
                        );
                        Color c = targetColors[i];
                        c.a = targetColors[i].a * shimmer;
                        beamRenderers[i].color = c;

                        // Subtle shake
                        float rotOffset = GetShakeOffset(i, elapsed);
                        if (originalLocalRotations != null && i < originalLocalRotations.Length && originalLocalRotations[i] != default(Quaternion))
                        {
                            beamRenderers[i].transform.localRotation = originalLocalRotations[i] * Quaternion.Euler(0f, 0f, rotOffset);
                        }
                    }
                    else
                    {
                        // Graceful Settle / Fade Out stage
                        float p = Mathf.Clamp01((elapsed - fadeStartTime) / beamFadeDuration);
                        float fade = 1f - p;

                        beamRenderers[i].transform.localScale = new Vector3(
                            targetScales[i].x * fade,
                            targetScales[i].y,
                            1f
                        );
                        Color c = targetColors[i];
                        c.a = targetColors[i].a * fade;
                        beamRenderers[i].color = c;

                        // Subtle shake fades out with beam
                        float rotOffset = GetShakeOffset(i, elapsed);
                        if (originalLocalRotations != null && i < originalLocalRotations.Length && originalLocalRotations[i] != default(Quaternion))
                        {
                            beamRenderers[i].transform.localRotation = originalLocalRotations[i] * Quaternion.Euler(0f, 0f, rotOffset * fade);
                        }
                    }
                }

                yield return null;
            }

            ResetBeams();
        }
    }
}
