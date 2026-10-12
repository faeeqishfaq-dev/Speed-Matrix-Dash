using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace GeometryDash.Player
{
    public class PlayerDeathController : MonoBehaviour
    {
        [Header("DEATH")]
        [SerializeField] private float restartDelay = 1.0f;
        [SerializeField] private GameObject deathFragmentPrefab;
        [SerializeField] private int fragmentCount = 12;
        [SerializeField] private float explosionForce = 15f;
        [SerializeField] private ParticleSystem deathParticles;

        private CubePlayerController playerController;
        private PlayerInputController inputController;
        private PlayerVFXController vfxController;
        
        [SerializeField] private GameObject visualCube;

        private void Awake()
        {
            playerController = GetComponent<CubePlayerController>();
            inputController = GetComponent<PlayerInputController>();
            vfxController = GetComponentInChildren<PlayerVFXController>();
        }

        [Header("SOLID COLLISION")]
        [Tooltip("Minimum Y value of a contact normal to be considered a safe top surface.")]
        [SerializeField] private float safeGroundNormalY = 0.85f;
        private int groundLayerIndex = -1;

        private void Start()
        {
            groundLayerIndex = LayerMask.NameToLayer("Ground");

            // Attach and play respawn VFX
            PlayerRespawnVFX respawnVFX = gameObject.AddComponent<PlayerRespawnVFX>();
            if (visualCube != null)
            {
                respawnVFX.PlayRespawnVFX(visualCube.transform);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Hazard") && playerController.IsAlive)
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            EvaluateSolidCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            EvaluateSolidCollision(collision);
        }

        private void EvaluateSolidCollision(Collision2D collision)
        {
            if (!playerController.IsAlive) return;

            if (collision.gameObject.CompareTag("Hazard"))
            {
                Die();
                return;
            }

            int targetLayer = groundLayerIndex != -1 ? groundLayerIndex : 8;

            if (collision.gameObject.layer == targetLayer)
            {
                for (int i = 0; i < collision.contactCount; i++)
                {
                    ContactPoint2D contact = collision.GetContact(i);
                    if (contact.normal.y < safeGroundNormalY)
                    {
                        Die();
                        break;
                    }
                }
            }
        }

        public void Die()
        {
            if (!playerController.IsAlive) return;
            
            playerController.IsAlive = false;
            inputController.InputEnabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            if (vfxController != null) vfxController.StopAllVFX();

            if (visualCube != null) visualCube.SetActive(false);
            
            // Trigger Camera Shake
            GeometryDash.Camera.SimpleCameraController cam = Object.FindObjectOfType<GeometryDash.Camera.SimpleCameraController>();
            if (cam != null)
            {
                cam.PlayDeathShake();
            }

            SpawnFragments();

            StartCoroutine(RestartRoutine());
        }

        private void SpawnFragments()
        {
            Vector3 center = transform.position;

            // 1. Initial bright flash
            SpawnFlash(center);

            // 2. Large filled cyan circular disc
            SpawnDeathDisc(center);

            // 3. Green geometric fragments
            SpawnGeometricFragments(center);

            // 4. Tiny square particles
            SpawnTinyDeathParticles(center);
        }

        private void SpawnFlash(Vector3 center)
        {
            GameObject flash = new GameObject("DeathFlash");
            flash.transform.position = center;
            SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
            sr.sprite = VFXHelper.GetCircleSprite();
            sr.color = Color.white;
            flash.transform.localScale = Vector3.one * 1.5f;
            StartCoroutine(FlashRoutine(flash));
        }

        private IEnumerator FlashRoutine(GameObject flash)
        {
            SpriteRenderer sr = flash.GetComponent<SpriteRenderer>();
            float duration = 0.08f;
            float t = 0;
            while(t < duration)
            {
                t += Time.deltaTime;
                sr.color = new Color(1, 1, 1, 1f - (t/duration));
                flash.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.zero, t/duration);
                yield return null;
            }
            Destroy(flash);
        }

        private void SpawnDeathDisc(Vector3 center)
        {
            GameObject disc = new GameObject("DeathDisc");
            disc.transform.position = center;
            // Push back slightly in Z so fragments render in front
            disc.transform.position += new Vector3(0, 0, 1f); 
            SpriteRenderer sr = disc.AddComponent<SpriteRenderer>();
            sr.sprite = VFXHelper.GetCircleSprite();
            Color c = new Color(0.4f, 1f, 1f, 0.6f); // Pale cyan, semi transparent
            sr.color = c;
            disc.transform.localScale = Vector3.zero;
            StartCoroutine(DiscRoutine(disc, c));
        }

        private IEnumerator DiscRoutine(GameObject disc, Color baseColor)
        {
            SpriteRenderer sr = disc.GetComponent<SpriteRenderer>();
            float t = 0;
            float maxTime = 0.45f;
            float maxRadius = 4.5f; // diameter = 4.5
            
            while(t < maxTime)
            {
                t += Time.deltaTime;
                float pct = t / maxTime;
                
                // Rapid expansion early
                float scale = Mathf.Lerp(0f, maxRadius, Mathf.Pow(pct, 0.3f));
                disc.transform.localScale = new Vector3(scale, scale, 1f);
                
                // Fade late
                float alpha = Mathf.Lerp(baseColor.a, 0f, Mathf.Pow(pct, 2f));
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                
                yield return null;
            }
            Destroy(disc);
        }

        private void SpawnGeometricFragments(Vector3 center)
        {
            int count = Random.Range(8, 13);
            for(int i = 0; i < count; i++)
            {
                GameObject frag = new GameObject("GreenFragment");
                frag.transform.position = center + (Vector3)Random.insideUnitCircle * 0.2f;
                // Pull forward slightly
                frag.transform.position -= new Vector3(0, 0, 1f);
                SpriteRenderer sr = frag.AddComponent<SpriteRenderer>();
                sr.sprite = VFXHelper.GetSquareSprite();
                
                // Bright green to match Geometry Dash cube exterior
                sr.color = new Color(0.2f, 1f, 0.2f, 1f);
                
                // Geometric shapes
                float width = Random.Range(0.15f, 0.45f);
                float height = Random.Range(0.15f, 0.45f);
                frag.transform.localScale = new Vector3(width, height, 1f);
                
                // Bias direction up and out
                Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.2f, 1.2f)).normalized;
                float speed = Random.Range(8f, 18f);
                Vector3 vel = dir * speed;
                
                float rot = Random.Range(-720f, 720f);
                
                StartCoroutine(FragRoutine(frag, vel, rot));
            }
        }

        private IEnumerator FragRoutine(GameObject frag, Vector3 vel, float rotSpeed)
        {
            float t = 0;
            float maxTime = 0.8f;
            Vector3 gravity = new Vector3(0, -35f, 0); // Fast arcade gravity
            
            SpriteRenderer sr = frag.GetComponent<SpriteRenderer>();
            
            while(t < maxTime)
            {
                t += Time.deltaTime;
                
                vel += gravity * Time.deltaTime;
                frag.transform.position += vel * Time.deltaTime;
                frag.transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
                
                // Fade out at end
                if (t > maxTime - 0.2f)
                {
                    float fadePct = (t - (maxTime - 0.2f)) / 0.2f;
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - fadePct);
                }
                
                yield return null;
            }
            Destroy(frag);
        }

        private void SpawnTinyDeathParticles(Vector3 center)
        {
            int count = Random.Range(15, 30);
            for(int i = 0; i < count; i++)
            {
                GameObject p = new GameObject("TinyParticle");
                p.transform.position = center + (Vector3)Random.insideUnitCircle * 0.3f;
                // Push between disc and fragments
                p.transform.position -= new Vector3(0, 0, 0.5f);
                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = VFXHelper.GetSquareSprite();
                
                float r = Random.value;
                if (r < 0.4f) sr.color = Color.white;
                else if (r < 0.8f) sr.color = new Color(0.4f, 1f, 1f, 1f); // cyan
                else sr.color = new Color(0.4f, 1f, 0.4f, 1f); // light green
                
                p.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);
                
                Vector2 dir = Random.insideUnitCircle.normalized;
                float speed = Random.Range(5f, 25f);
                Vector3 vel = dir * speed;
                
                StartCoroutine(TinyParticleRoutine(p, vel));
            }
        }

        private IEnumerator TinyParticleRoutine(GameObject p, Vector3 vel)
        {
            float t = 0;
            float maxTime = Random.Range(0.2f, 0.4f);
            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            
            while(t < maxTime)
            {
                t += Time.deltaTime;
                p.transform.position += vel * Time.deltaTime;
                vel *= 0.95f; // drag
                
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - (t/maxTime));
                yield return null;
            }
            Destroy(p);
        }

        private IEnumerator RestartRoutine()
        {
            yield return new WaitForSeconds(restartDelay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
