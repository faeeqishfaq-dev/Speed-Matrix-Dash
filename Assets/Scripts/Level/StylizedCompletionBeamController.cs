using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace GeometryDash.Level
{
    [DisallowMultipleComponent]
    public class StylizedCompletionBeamController : MonoBehaviour
    {
        [Header("SINGLE COMPLETION BEAM")]
        [Tooltip("Reference to the existing VisualEffect component.")]
        public VisualEffect visualEffect;

        [Tooltip("Total duration for the beam animation sequence (in seconds).")]
        public float beamDuration = 3.0f;

        [Header("ANIMATION CURVES")]
        [Tooltip("Controls beam width/thickness over the 3-second duration (0 to 1 normalized).")]
        public AnimationCurve beamThicknessCurve = CreateDefaultThicknessCurve();

        [Tooltip("Controls beam brightness and color intensity over the 3-second duration (0 to 1 normalized).")]
        public AnimationCurve beamIntensityCurve = CreateDefaultIntensityCurve();

        [Header("MULTIPLIERS")]
        [Tooltip("Peak thickness multiplier at beam start (gives high pressure full power).")]
        public float startThicknessMultiplier = 2.0f;

        [Tooltip("Peak intensity multiplier at beam start.")]
        public float startIntensityMultiplier = 1.0f;

        [Header("BEAM DIRECTION & REACH")]
        [Tooltip("Local direction to fire the beam. In 2D, firing to the left across the screen requires local +Z pointing left (Euler Y = -90).")]
        public Vector3 beamLocalEuler = new Vector3(0f, -90f, 0f);

        [Tooltip("Beam length in world units to span across the visible screen.")]
        public float beamLength = 32f;

        [Header("INTERNAL LIGHTS")]
        [Tooltip("Whether to animate the internal point lights attached to the beam.")]
        public bool animateInternalLights = true;

        // Cached original parameters
        private Vector3 baseBeamsScale = Vector3.one;
        private float baseBeamCoreSize = 3.0f;
        private float baseFlashSize = 10f;
        private float baseElectricFlashSize = 10f;
        private float baseDarkFlashSize = 10f;
        private float baseParticlesSize = 1f;

        private Vector4 baseBeamCoreColor = new Vector4(7.91f, 2.11f, 0.70f, 1f);
        private Vector4 baseElectricBeamColor = new Vector4(6.27f, 12.81f, 47.93f, 1f);
        private Vector4 baseDarkBeamColor = new Vector4(0.05f, 0.006f, 0.006f, 1f);
        private Vector4 baseParticlesColor = new Vector4(25.38f, 5.89f, 1.33f, 1f);

        private float baseParticlesRate = 40f;
        private float baseFlashRate = 16f;
        private float baseElectricFlashRate = 32f;
        private float baseDarkFlashRate = 8f;
        private float baseInitialParticlesRate = 15f;
        private float baseImpactFlashRate = 10f;
        private float baseImpactParticlesRate = 20f;
        private float baseImpactRingRate = 5f;

        private Light[] internalLights;
        private float[] baseLightIntensities;

        private ParticleSystem[] childParticleSystems;

        private Coroutine beamRoutine;
        private bool isInitialized = false;
        private bool isBeamActive = false;

        private void Awake()
        {
            InitializeReferences();
            ResetBeam();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            InitializeReferences();
            ResetBeam();
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            isBeamActive = false;
        }

        public bool HasValidBeam()
        {
            InitializeReferences();
            return visualEffect != null;
        }

        public void InitializeReferences()
        {
            if (visualEffect == null)
            {
                visualEffect = GetComponentInChildren<VisualEffect>(true);
            }

            // Cache child particle systems (the Light emitters placed along the beam)
            childParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
            if (childParticleSystems != null)
            {
                foreach (var ps in childParticleSystems)
                {
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.useUnscaledTime = true;
                    }
                }
            }

            // Disable any VFXPropertyBinder / VFXTransformBinder so external target bindings cannot override BeamEndPoint_position
            var binders = GetComponentsInChildren<UnityEngine.VFX.Utility.VFXPropertyBinder>(true);
            if (binders != null)
            {
                foreach (var binder in binders)
                {
                    binder.enabled = false;
                }
            }

            if (visualEffect != null && !isInitialized)
            {
                // Ensure default simulation settings
                visualEffect.pause = false;
                visualEffect.playRate = 1.0f;

                // Cache base parameter values
                if (visualEffect.HasVector3("BeamsScale"))
                    baseBeamsScale = visualEffect.GetVector3("BeamsScale");
                if (visualEffect.HasFloat("BeamCoreSize"))
                    baseBeamCoreSize = visualEffect.GetFloat("BeamCoreSize");
                if (visualEffect.HasFloat("FlashSize"))
                    baseFlashSize = visualEffect.GetFloat("FlashSize");
                if (visualEffect.HasFloat("ElectricFlashSize"))
                    baseElectricFlashSize = visualEffect.GetFloat("ElectricFlashSize");
                if (visualEffect.HasFloat("DarkFlashSize"))
                    baseDarkFlashSize = visualEffect.GetFloat("DarkFlashSize");
                if (visualEffect.HasFloat("ParticlesSize"))
                    baseParticlesSize = visualEffect.GetFloat("ParticlesSize");

                if (visualEffect.HasVector4("BeamCoreColor"))
                    baseBeamCoreColor = visualEffect.GetVector4("BeamCoreColor");
                if (visualEffect.HasVector4("ElectricBeamColor"))
                    baseElectricBeamColor = visualEffect.GetVector4("ElectricBeamColor");
                if (visualEffect.HasVector4("DarkBeamColor"))
                    baseDarkBeamColor = visualEffect.GetVector4("DarkBeamColor");
                if (visualEffect.HasVector4("ParticlesColor"))
                    baseParticlesColor = visualEffect.GetVector4("ParticlesColor");

                if (visualEffect.HasFloat("ParticlesRate"))
                    baseParticlesRate = visualEffect.GetFloat("ParticlesRate");
                if (visualEffect.HasFloat("FlashRate"))
                    baseFlashRate = visualEffect.GetFloat("FlashRate");
                if (visualEffect.HasFloat("ElectricFlashRate"))
                    baseElectricFlashRate = visualEffect.GetFloat("ElectricFlashRate");
                if (visualEffect.HasFloat("DarkFlashRate"))
                    baseDarkFlashRate = visualEffect.GetFloat("DarkFlashRate");
                if (visualEffect.HasFloat("InitialParticlesRate"))
                    baseInitialParticlesRate = visualEffect.GetFloat("InitialParticlesRate");
                if (visualEffect.HasFloat("ImpactFlashRate"))
                    baseImpactFlashRate = visualEffect.GetFloat("ImpactFlashRate");
                if (visualEffect.HasFloat("ImpactParticlesRate"))
                    baseImpactParticlesRate = visualEffect.GetFloat("ImpactParticlesRate");
                if (visualEffect.HasFloat("ImpactRingRate"))
                    baseImpactRingRate = visualEffect.GetFloat("ImpactRingRate");

                internalLights = GetComponentsInChildren<Light>(true);
                if (internalLights != null && internalLights.Length > 0)
                {
                    baseLightIntensities = new float[internalLights.Length];
                    for (int i = 0; i < internalLights.Length; i++)
                    {
                        if (internalLights[i] != null)
                            baseLightIntensities[i] = internalLights[i].intensity;
                    }
                }

                isInitialized = true;
            }
        }

        private void Update()
        {
            if (isBeamActive && Time.timeScale <= 0.001f)
            {
                UpdateShaderTime();
            }
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (isBeamActive && Time.timeScale <= 0.001f)
            {
                UpdateShaderTime();
            }
        }

        private void UpdateShaderTime()
        {
            // When game timeScale is 0 during milestone completion, Unity's internal shader _Time freezes.
            // Explicitly drive shader time using unscaled real time so LaserShader UV distortion & scroll stay alive!
            float t = Time.unscaledTime;
            Shader.SetGlobalVector("_Time", new Vector4(t / 20f, t, t * 2f, t * 3f));
            Shader.SetGlobalVector("_SinTime", new Vector4(Mathf.Sin(t / 8f), Mathf.Sin(t / 4f), Mathf.Sin(t / 2f), Mathf.Sin(t)));
            Shader.SetGlobalVector("_CosTime", new Vector4(Mathf.Cos(t / 8f), Mathf.Cos(t / 4f), Mathf.Cos(t / 2f), Mathf.Cos(t)));
        }

        public void PlayBeam()
        {
            InitializeReferences();
            if (visualEffect == null) return;

            if (beamRoutine != null)
            {
                StopCoroutine(beamRoutine);
                beamRoutine = null;
            }

            beamRoutine = StartCoroutine(AnimateBeamRoutine());
        }

        public void StopBeam()
        {
            if (beamRoutine != null)
            {
                StopCoroutine(beamRoutine);
                beamRoutine = null;
            }

            isBeamActive = false;
            SetThicknessAndIntensity(0f, 0f);

            if (visualEffect != null)
            {
                visualEffect.Stop();
                visualEffect.pause = false;
            }

            if (childParticleSystems != null)
            {
                foreach (var ps in childParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }
        }

        public void ResetBeam()
        {
            if (beamRoutine != null)
            {
                StopCoroutine(beamRoutine);
                beamRoutine = null;
            }

            isBeamActive = false;
            InitializeReferences();
            if (visualEffect == null) return;

            // Align beam direction
            transform.localEulerAngles = beamLocalEuler;

            if (visualEffect.HasVector3("BeamEndPoint_position"))
            {
                visualEffect.SetVector3("BeamEndPoint_position", new Vector3(0f, 0f, beamLength));
            }

            // Restore base parameters so cached values are clean
            RestoreOriginalValues();

            // Hide before trigger
            SetThicknessAndIntensity(0f, 0f);

            visualEffect.Stop();
            visualEffect.pause = false;
            visualEffect.playRate = 1.0f;

            // Stop and clear child particle systems
            if (childParticleSystems != null)
            {
                foreach (var ps in childParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            // CHECK 4: Single clean Reinit on reset/replay only, NEVER every frame
            visualEffect.Reinit();
        }

        private void RestoreOriginalValues()
        {
            if (visualEffect == null) return;

            if (visualEffect.HasVector3("BeamsScale"))
                visualEffect.SetVector3("BeamsScale", baseBeamsScale);
            if (visualEffect.HasFloat("BeamCoreSize"))
                visualEffect.SetFloat("BeamCoreSize", baseBeamCoreSize);
            if (visualEffect.HasFloat("FlashSize"))
                visualEffect.SetFloat("FlashSize", baseFlashSize);
            if (visualEffect.HasFloat("ElectricFlashSize"))
                visualEffect.SetFloat("ElectricFlashSize", baseElectricFlashSize);
            if (visualEffect.HasFloat("DarkFlashSize"))
                visualEffect.SetFloat("DarkFlashSize", baseDarkFlashSize);
            if (visualEffect.HasFloat("ParticlesSize"))
                visualEffect.SetFloat("ParticlesSize", baseParticlesSize);

            if (visualEffect.HasVector4("BeamCoreColor"))
                visualEffect.SetVector4("BeamCoreColor", baseBeamCoreColor);
            if (visualEffect.HasVector4("ElectricBeamColor"))
                visualEffect.SetVector4("ElectricBeamColor", baseElectricBeamColor);
            if (visualEffect.HasVector4("DarkBeamColor"))
                visualEffect.SetVector4("DarkBeamColor", baseDarkBeamColor);
            if (visualEffect.HasVector4("ParticlesColor"))
                visualEffect.SetVector4("ParticlesColor", baseParticlesColor);

            if (visualEffect.HasFloat("ParticlesRate"))
                visualEffect.SetFloat("ParticlesRate", baseParticlesRate);
            if (visualEffect.HasFloat("FlashRate"))
                visualEffect.SetFloat("FlashRate", baseFlashRate);
            if (visualEffect.HasFloat("ElectricFlashRate"))
                visualEffect.SetFloat("ElectricFlashRate", baseElectricFlashRate);
            if (visualEffect.HasFloat("DarkFlashRate"))
                visualEffect.SetFloat("DarkFlashRate", baseDarkFlashRate);
            if (visualEffect.HasFloat("InitialParticlesRate"))
                visualEffect.SetFloat("InitialParticlesRate", baseInitialParticlesRate);
            if (visualEffect.HasFloat("ImpactFlashRate"))
                visualEffect.SetFloat("ImpactFlashRate", baseImpactFlashRate);
            if (visualEffect.HasFloat("ImpactParticlesRate"))
                visualEffect.SetFloat("ImpactParticlesRate", baseImpactParticlesRate);
            if (visualEffect.HasFloat("ImpactRingRate"))
                visualEffect.SetFloat("ImpactRingRate", baseImpactRingRate);

            if (internalLights != null && baseLightIntensities != null)
            {
                for (int i = 0; i < internalLights.Length; i++)
                {
                    if (internalLights[i] != null)
                        internalLights[i].intensity = baseLightIntensities[i];
                }
            }
        }

        private IEnumerator AnimateBeamRoutine()
        {
            isBeamActive = true;

            // Ensure proper orientation and reach
            transform.localEulerAngles = beamLocalEuler;

            if (visualEffect.HasVector3("BeamEndPoint_position"))
            {
                visualEffect.SetVector3("BeamEndPoint_position", new Vector3(0f, 0f, beamLength));
            }
            if (visualEffect.HasFloat("Duration"))
            {
                visualEffect.SetFloat("Duration", beamDuration);
            }

            // Restore all base rates and scales so simulation starts fresh
            RestoreOriginalValues();

            // CHECK 2: VisualEffect.pause MUST be false during beam animation!
            visualEffect.pause = false;

            // CHECK 3: Ensure original intended playRate
            visualEffect.playRate = 1.0f;

            // Start child particle systems (the Light emitters placed along the beam)
            if (childParticleSystems != null)
            {
                foreach (var ps in childParticleSystems)
                {
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.useUnscaledTime = true;
                        ps.Clear();
                        ps.Play();
                    }
                }
            }

            // Start full pressure: 100% thickness & intensity
            SetThicknessAndIntensity(1.0f * startThicknessMultiplier, 1.0f * startIntensityMultiplier);

            // CHECK 5: Trigger VFX Play event ONCE when beam starts
            visualEffect.SendEvent("OnPlay");
            visualEffect.Play();

            // Initial prime step for instant impact if game time is frozen
            if (Time.timeScale <= 0.001f)
            {
                visualEffect.Simulate(0.016f, 1);
            }

            float elapsed = 0f;
            while (elapsed < beamDuration)
            {
                float dt = Time.unscaledDeltaTime;
                if (dt <= 0f) dt = 0.016f;
                elapsed += dt;

                float normTime = Mathf.Clamp01(elapsed / beamDuration);

                // Sample curves
                float thicknessNorm = beamThicknessCurve != null ? beamThicknessCurve.Evaluate(normTime) : (1f - normTime);
                float intensityNorm = beamIntensityCurve != null ? beamIntensityCurve.Evaluate(normTime) : (1f - normTime);

                float currentThickness = thicknessNorm * startThicknessMultiplier;
                float currentIntensity = intensityNorm * startIntensityMultiplier;

                SetThicknessAndIntensity(currentThickness, currentIntensity);

                // CHECK 1: Advance internal particle simulation using unscaled delta time ONLY when game time is paused
                if (Time.timeScale <= 0.001f && visualEffect != null)
                {
                    visualEffect.Simulate(dt, 1);
                }

                yield return null;
            }

            // CHECK 6: T = 3.00s: Only stop after the 3-second beam envelope completes
            SetThicknessAndIntensity(0f, 0f);
            visualEffect.Stop();
            visualEffect.pause = false;

            if (childParticleSystems != null)
            {
                foreach (var ps in childParticleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            isBeamActive = false;
            beamRoutine = null;
        }

        private void SetThicknessAndIntensity(float thickness, float intensity)
        {
            if (visualEffect == null) return;

            // 1. Beam Width / Thickness scaling (X and Z cross-section, preserve Y length)
            if (visualEffect.HasVector3("BeamsScale"))
            {
                Vector3 scale = new Vector3(
                    baseBeamsScale.x * thickness,
                    baseBeamsScale.y,
                    baseBeamsScale.z * thickness
                );
                visualEffect.SetVector3("BeamsScale", scale);
            }

            if (visualEffect.HasFloat("BeamCoreSize"))
                visualEffect.SetFloat("BeamCoreSize", baseBeamCoreSize * thickness);
            if (visualEffect.HasFloat("FlashSize"))
                visualEffect.SetFloat("FlashSize", baseFlashSize * thickness);
            if (visualEffect.HasFloat("ElectricFlashSize"))
                visualEffect.SetFloat("ElectricFlashSize", baseElectricFlashSize * thickness);
            if (visualEffect.HasFloat("DarkFlashSize"))
                visualEffect.SetFloat("DarkFlashSize", baseDarkFlashSize * thickness);
            if (visualEffect.HasFloat("ParticlesSize"))
                visualEffect.SetFloat("ParticlesSize", baseParticlesSize * Mathf.Lerp(0.3f, 1f, thickness));

            // 2. Brightness / Intensity (HDR Colors)
            float safeIntensity = Mathf.Max(0f, intensity);
            if (visualEffect.HasVector4("BeamCoreColor"))
                visualEffect.SetVector4("BeamCoreColor", baseBeamCoreColor * safeIntensity);
            if (visualEffect.HasVector4("ElectricBeamColor"))
                visualEffect.SetVector4("ElectricBeamColor", baseElectricBeamColor * safeIntensity);
            if (visualEffect.HasVector4("DarkBeamColor"))
                visualEffect.SetVector4("DarkBeamColor", baseDarkBeamColor * safeIntensity);
            if (visualEffect.HasVector4("ParticlesColor"))
                visualEffect.SetVector4("ParticlesColor", baseParticlesColor * safeIntensity);

            // Note: We do NOT overwrite ParticlesRate, FlashRate, etc. every frame!
            // This leaves the internal spawner simulation alive and unchoked throughout the 3s lifetime.

            // 3. Point Lights if present
            if (animateInternalLights && internalLights != null && baseLightIntensities != null)
            {
                for (int i = 0; i < internalLights.Length; i++)
                {
                    if (internalLights[i] != null)
                    {
                        internalLights[i].intensity = baseLightIntensities[i] * safeIntensity;
                        internalLights[i].enabled = safeIntensity > 0.01f;
                    }
                }
            }
        }

        public static AnimationCurve CreateDefaultThicknessCurve()
        {
            Keyframe[] keys = new Keyframe[]
            {
                new Keyframe(0.00f, 1.00f, 0.0000f, 0.0000f),
                new Keyframe(0.10f, 1.00f, 0.0000f, 0.0000f),
                new Keyframe(0.20f, 0.97f, -0.9565f, -0.9565f),
                new Keyframe(0.33f, 0.78f, -1.3000f, -1.3000f),
                new Keyframe(0.50f, 0.58f, -1.1765f, -1.1765f),
                new Keyframe(0.67f, 0.38f, -1.2000f, -1.2000f),
                new Keyframe(0.80f, 0.22f, -1.1923f, -1.1923f),
                new Keyframe(0.93f, 0.07f, -1.1000f, -1.1000f),
                new Keyframe(1.00f, 0.00f, -1.0000f, -1.0000f)
            };
            return new AnimationCurve(keys);
        }

        public static AnimationCurve CreateDefaultIntensityCurve()
        {
            Keyframe[] keys = new Keyframe[]
            {
                new Keyframe(0.00f, 1.00f, 0.0000f, 0.0000f),
                new Keyframe(0.15f, 1.00f, 0.0000f, 0.0000f),
                new Keyframe(0.33f, 0.95f, -0.4286f, -0.4286f),
                new Keyframe(0.50f, 0.85f, -0.8824f, -0.8824f),
                new Keyframe(0.67f, 0.65f, -1.3333f, -1.3333f),
                new Keyframe(0.80f, 0.45f, -1.7308f, -1.7308f),
                new Keyframe(0.93f, 0.20f, -2.2500f, -2.2500f),
                new Keyframe(1.00f, 0.00f, -2.8571f, -2.8571f)
            };
            return new AnimationCurve(keys);
        }
    }
}
