using UnityEngine;

namespace GeometryDash.Camera
{
    public class SimpleCameraController : MonoBehaviour
    {
        [Header("FOLLOW SETTINGS")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(4f, 1f, -10f);
        [SerializeField] private float smoothSpeed = 15f;
        [SerializeField] private bool followY = false;

        [Header("DEATH CAMERA SHAKE")]
        [SerializeField] private bool enableDeathShake = true;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float shakeStrength = 0.08f;
        [SerializeField] private float shakeFrequency = 25f;
        [SerializeField] private float rotationStrength = 0.0f;

        private float currentShakeTime = 0f;
        private Vector3 basePosition;

        private void Start()
        {
            basePosition = transform.position;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 desiredPosition = new Vector3(
                    target.position.x + offset.x,
                    followY ? target.position.y + offset.y : offset.y,
                    offset.z
                );

                basePosition = Vector3.Lerp(basePosition, desiredPosition, smoothSpeed * Time.deltaTime);
            }
            else
            {
                // If no target, basePosition stays where it is, or we lerp from current position (useful if target dies)
            }

            Vector3 finalPosition = basePosition;
            Quaternion finalRotation = Quaternion.identity;

            if (enableDeathShake && currentShakeTime > 0)
            {
                currentShakeTime -= Time.unscaledDeltaTime;
                float decay = Mathf.Clamp01(currentShakeTime / shakeDuration);
                float strength = shakeStrength * decay;
                
                // Smooth Perlin noise based shake
                float time = Time.unscaledTime * shakeFrequency;
                float shakeX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
                float shakeY = (Mathf.PerlinNoise(time, 100f) - 0.5f) * 2f;
                
                finalPosition += new Vector3(shakeX, shakeY, 0f) * strength;

                if (rotationStrength > 0)
                {
                    float rotAmp = rotationStrength * decay;
                    float shakeRot = (Mathf.PerlinNoise(time, 200f) - 0.5f) * 2f;
                    finalRotation = Quaternion.Euler(0, 0, shakeRot * rotAmp);
                }
            }

            transform.position = finalPosition;
            transform.rotation = finalRotation;
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            // Snap immediately
            basePosition = new Vector3(target.position.x + offset.x, followY ? target.position.y + offset.y : offset.y, offset.z);
            transform.position = basePosition;
        }

        public void PlayDeathShake()
        {
            if (enableDeathShake)
            {
                currentShakeTime = shakeDuration;
            }
        }
    }
}
