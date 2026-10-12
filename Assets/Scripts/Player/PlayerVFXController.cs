using UnityEngine;

namespace GeometryDash.Player
{
    public class PlayerVFXController : MonoBehaviour
    {
        [Header("CUBE MOVEMENT PARTICLES")]
        [SerializeField] private bool enableMovementParticles = true;
        [SerializeField] private float emissionRate = 25f;
        [SerializeField] private float particleLifetime = 0.35f;
        [SerializeField] private float minParticleSize = 0.04f;
        [SerializeField] private float maxParticleSize = 0.12f;
        [SerializeField] private float backwardVelocity = -0.5f;
        [SerializeField] private float verticalSpread = 0.1f;
        [SerializeField] private Color particleColor = new Color(0.81f, 1f, 1f, 1f); // #CFFFFF
        [SerializeField] private float gravityModifier = 0.05f;

        [Header("LEGACY VFX (Will be disabled)")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem runningSmoke;
        [SerializeField] private ParticleSystem landingDust;

        private ParticleSystem movementParticles;
        private CubePlayerController playerController;
        private bool wasGrounded;

        private void Awake()
        {
            playerController = GetComponentInParent<CubePlayerController>();
            
            // Disable old legacy trails
            if (trailRenderer != null) trailRenderer.enabled = false;
            if (runningSmoke != null) runningSmoke.gameObject.SetActive(false);
            
            SetupMovementParticles();
        }

        private void SetupMovementParticles()
        {
            GameObject emitterObj = new GameObject("MovementTrailEmitter");
            emitterObj.transform.SetParent(playerController.transform);
            
            // Position: lower-back / bottom-left of the Cube
            emitterObj.transform.localPosition = new Vector3(-0.45f, -0.45f, 0f);

            movementParticles = emitterObj.AddComponent<ParticleSystem>();
            // Prevent duration assignment error by stopping the auto-played system
            movementParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            // MAIN MODULE
            var main = movementParticles.main;
            main.playOnAwake = true;
            main.duration = 1f;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = particleLifetime;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(minParticleSize, maxParticleSize);
            main.startColor = particleColor;
            main.gravityModifier = gravityModifier;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            
            // EMISSION MODULE
            var emission = movementParticles.emission;
            emission.rateOverTime = emissionRate;

            // SHAPE MODULE
            var shape = movementParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.01f, verticalSpread, 1f); // tiny box

            // VELOCITY OVER LIFETIME MODULE
            var vel = movementParticles.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local; // Local to the emitter, which is rotated 0
            // Unity requires ALL axes to share the exact same MinMaxCurve mode
            vel.x = new ParticleSystem.MinMaxCurve(backwardVelocity - 0.2f, backwardVelocity + 0.2f);
            vel.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.3f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            // SIZE OVER LIFETIME MODULE
            var sizeOverLife = movementParticles.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(0.7f, 0.35f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // COLOR OVER LIFETIME MODULE
            var colorOverLife = movementParticles.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            // RENDERER MODULE
            var renderer = movementParticles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            // Try to load the square particle texture if we generated one, otherwise fallback to VFXHelper
            Sprite squareSprite = Resources.Load<Sprite>("SquareParticle");
            if (squareSprite == null) squareSprite = VFXHelper.GetSquareSprite();
            
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = squareSprite.texture;
            renderer.material = mat;
            
            if (enableMovementParticles) movementParticles.Play();
        }

        private void Update()
        {
            if (movementParticles != null)
            {
                // Update properties dynamically for Inspector tweaking
                var main = movementParticles.main;
                main.startLifetime = particleLifetime;
                main.startSize = new ParticleSystem.MinMaxCurve(minParticleSize, maxParticleSize);
                main.startColor = particleColor;
                main.gravityModifier = gravityModifier;
                
                var emission = movementParticles.emission;
                emission.rateOverTime = emissionRate;

                var shape = movementParticles.shape;
                shape.scale = new Vector3(0.01f, verticalSpread, 1f);

                var vel = movementParticles.velocityOverLifetime;
                vel.x = new ParticleSystem.MinMaxCurve(backwardVelocity - 0.2f, backwardVelocity + 0.2f);
                vel.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.3f);
                vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            }

            if (!playerController.IsAlive)
            {
                if (movementParticles != null && movementParticles.isPlaying) movementParticles.Stop();
                return;
            }

            // In Geometry Dash, jump/fall still emits the trail.
            // If the user wants it continuously while alive, we just make sure it's playing.
            if (enableMovementParticles)
            {
                if (!movementParticles.isPlaying) movementParticles.Play();
            }
            else
            {
                if (movementParticles.isPlaying) movementParticles.Stop();
            }
        }

        public void StopAllVFX()
        {
            if (trailRenderer != null) trailRenderer.emitting = false;
            if (runningSmoke != null && runningSmoke.isPlaying) runningSmoke.Stop();
            if (movementParticles != null && movementParticles.isPlaying) movementParticles.Stop();
        }

        public void ResetVFX()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = false;
            }
            if (movementParticles != null)
            {
                movementParticles.Clear();
                if (enableMovementParticles) movementParticles.Play();
            }
        }
    }
}
