using UnityEngine;
using System.Collections;

namespace GeometryDash.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DeathFragment : MonoBehaviour
    {
        [SerializeField] private float minLifetime = 0.4f;
        [SerializeField] private float maxLifetime = 0.8f;

        public void Initialize(Vector2 center, float explosionForce)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            
            // Random direction outward from center
            Vector2 dir = ((Vector2)transform.position - center).normalized;
            
            // If perfectly centered, pick random
            if (dir == Vector2.zero)
            {
                dir = Random.insideUnitCircle.normalized;
            }

            rb.velocity = dir * explosionForce * Random.Range(0.8f, 1.3f);
            rb.angularVelocity = Random.Range(-1000f, 1000f);

            float lifetime = Random.Range(minLifetime, maxLifetime);
            StartCoroutine(ShrinkAndDestroy(lifetime));
        }

        private IEnumerator ShrinkAndDestroy(float lifetime)
        {
            Vector3 startScale = transform.localScale;
            float timer = 0f;

            while (timer < lifetime)
            {
                timer += Time.deltaTime;
                // Shrink out rapidly at the end of lifetime
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.Pow(timer / lifetime, 2f));
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
