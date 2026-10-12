using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace GeometryDash.Level
{
    public class LevelCompleteVFXController : MonoBehaviour
    {
        private MilestoneLevelCompleteController config;
        
        [Header("Scene References (Auto-generated)")]
        public Transform worldCanvas;
        public Transform screenCanvas;
        public TextMeshProUGUI completeText;
        public Sprite circleSprite;
        public Sprite squareSprite;
        public Sprite raySprite; // Triangle/polygon

        public void Initialize(MilestoneLevelCompleteController config, Vector3 origin)
        {
            this.config = config;
            transform.position = origin;
            
            StartCoroutine(SequenceRoutine());
        }

        private IEnumerator SequenceRoutine()
        {
            float t = 0;
            
            // 0.00-0.15 Initial finish flash
            SpawnFlash();

            // 0.10 Golden expanding circle begins
            yield return new WaitForSecondsRealtime(0.1f);
            SpawnGoldenBlast();

            // 0.15 Golden radial rays extend
            yield return new WaitForSecondsRealtime(0.05f);
            SpawnRays();

            // 0.20 Yellow square particles spread
            yield return new WaitForSecondsRealtime(0.05f);
            SpawnSquareParticles();

            // 1.3 Secondary bright pixel burst
            yield return new WaitForSecondsRealtime(1.1f);
            SpawnPixelBurst();

            // 1.5 LEVEL COMPLETE! text pops in
            yield return new WaitForSecondsRealtime(0.2f);
            ShowText();
            SpawnSecondaryBlast();

            // 1.5-2.4 Large golden celebration circles
            SpawnCelebrationCircles();

            // 1.7-3.0 White/cyan/pink floating orbs
            yield return new WaitForSecondsRealtime(0.2f);
            SpawnFloatingOrbs();
        }

        private void SpawnFlash()
        {
            GameObject flash = new GameObject("Flash");
            flash.transform.SetParent(worldCanvas, false);
            SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = Color.white;
            StartCoroutine(AnimateScaleAndAlpha(flash.transform, sr, 0f, 5f, 1f, 0f, 0.2f));
        }

        private void SpawnGoldenBlast()
        {
            GameObject blast = new GameObject("GoldenBlast");
            blast.transform.SetParent(worldCanvas, false);
            SpriteRenderer sr = blast.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = config.BlastColor;
            StartCoroutine(AnimateScaleAndAlpha(blast.transform, sr, 0f, config.BlastMaxScale, 0.8f, 0f, config.BlastDuration));
        }

        private void SpawnSecondaryBlast()
        {
            GameObject blast = new GameObject("SecondaryBlast");
            blast.transform.SetParent(worldCanvas, false);
            SpriteRenderer sr = blast.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = new Color(1f, 0.6f, 0f, 0.5f);
            StartCoroutine(AnimateScaleAndAlpha(blast.transform, sr, 0f, config.BlastMaxScale * 1.5f, 0.5f, 0f, config.BlastDuration));
        }

        private void SpawnRays()
        {
            for (int i = 0; i < config.RayCount; i++)
            {
                GameObject ray = new GameObject("Ray");
                ray.transform.SetParent(worldCanvas, false);
                SpriteRenderer sr = ray.AddComponent<SpriteRenderer>();
                sr.sprite = raySprite;
                sr.color = new Color(1f, 0.8f, 0f, 0.8f);
                
                // fan outward leftward mostly
                float angle = Random.Range(120f, 240f); 
                ray.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                float length = Random.Range(config.RayMinLength, config.RayMaxLength);
                float width = Random.Range(config.RayMinWidth, config.RayMaxWidth);
                
                StartCoroutine(AnimateRay(ray.transform, sr, width, length, config.RayDuration, config.RayFadeDuration));
            }
        }

        private IEnumerator AnimateRay(Transform t, SpriteRenderer sr, float width, float length, float inDur, float fadeDur)
        {
            float elapsed = 0;
            Color c = sr.color;
            while(elapsed < inDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / inDur;
                float easeOut = 1 - Mathf.Pow(1 - p, 3);
                t.localScale = new Vector3(width, length * easeOut, 1f);
                t.localPosition = t.up * (length * easeOut * 0.5f);
                yield return null;
            }
            
            elapsed = 0;
            while(elapsed < fadeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / fadeDur;
                sr.color = new Color(c.r, c.g, c.b, c.a * (1 - p));
                yield return null;
            }
            Destroy(t.gameObject);
        }

        private void SpawnSquareParticles()
        {
            for (int i = 0; i < config.ParticleCount; i++)
            {
                SpawnSingleParticle(config.ParticleSpeed, config.ParticleLifetime, false);
            }
        }

        private void SpawnPixelBurst()
        {
            for (int i = 0; i < 30; i++)
            {
                SpawnSingleParticle(config.ParticleSpeed * 0.5f, 0.3f, true);
            }
        }

        private void SpawnSingleParticle(float speed, float lifetime, bool isBurst)
        {
            GameObject p = new GameObject("Particle");
            p.transform.SetParent(worldCanvas, false);
            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            
            if (isBurst) sr.color = Color.Lerp(Color.white, Color.yellow, Random.value);
            else sr.color = Color.Lerp(new Color(1f, 0.8f, 0f), new Color(1f, 0.5f, 0f), Random.value);

            Vector3 dir = Random.insideUnitCircle.normalized;
            StartCoroutine(AnimateParticle(p.transform, sr, dir, speed * Random.Range(0.5f, 1.5f), lifetime, config.ParticleSize * Random.Range(0.5f, 1.5f)));
        }

        private IEnumerator AnimateParticle(Transform t, SpriteRenderer sr, Vector3 dir, float speed, float lifetime, float size)
        {
            t.localScale = Vector3.one * size;
            float elapsed = 0;
            Color c = sr.color;
            Vector3 pos = Vector3.zero;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / lifetime;
                
                pos += dir * speed * Time.unscaledDeltaTime;
                speed = Mathf.Lerp(speed, 0, Time.unscaledDeltaTime * 2f); // slow down
                
                t.localPosition = pos;
                sr.color = new Color(c.r, c.g, c.b, c.a * (1 - p));
                yield return null;
            }
            Destroy(t.gameObject);
        }

        private void ShowText()
        {
            completeText.gameObject.SetActive(true);
            completeText.text = config.LevelCompleteText;
            completeText.fontSize = config.FontSize;
            completeText.color = config.TextColor;
            completeText.outlineWidth = config.OutlineWidth;
            completeText.outlineColor = new Color32(0, 50, 0, 255);
            
            StartCoroutine(AnimateTextPop());
        }

        private IEnumerator AnimateTextPop()
        {
            float dur = config.PopDuration;
            float elapsed = 0;
            while(elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                // Overshoot 0.7 -> 1.15 -> 1.0
                float scale = 0f;
                if (p < 0.6f) scale = Mathf.Lerp(0.7f, 1.15f, p / 0.6f);
                else scale = Mathf.Lerp(1.15f, 1.0f, (p - 0.6f) / 0.4f);
                
                completeText.transform.localScale = Vector3.one * scale;
                completeText.alpha = Mathf.Clamp01(p * 2f);
                yield return null;
            }
            completeText.transform.localScale = Vector3.one;
        }

        private void SpawnCelebrationCircles()
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnOrb(Random.Range(3f, 8f), new Color(1f, 0.8f, 0f, 0.2f), Random.insideUnitCircle * 5f);
            }
        }

        private void SpawnFloatingOrbs()
        {
            for (int i = 0; i < config.OrbCount; i++)
            {
                Color[] colors = { Color.white, Color.cyan, new Color(1f, 0.7f, 0.8f), new Color(1f, 1f, 0.8f) };
                Color c = colors[Random.Range(0, colors.Length)];
                c.a = 0.3f;
                SpawnOrb(Random.Range(config.OrbMinSize, config.OrbMaxSize), c, Random.insideUnitCircle * 10f);
            }
        }

        private void SpawnOrb(float size, Color c, Vector3 offset)
        {
            GameObject orb = new GameObject("Orb");
            orb.transform.SetParent(worldCanvas, false);
            orb.transform.localPosition = offset;
            SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = c;
            
            Vector3 drift = Random.insideUnitCircle.normalized * config.OrbDriftSpeed;
            StartCoroutine(AnimateOrb(orb.transform, sr, size, drift, config.OrbLifetime));
        }

        private IEnumerator AnimateOrb(Transform t, SpriteRenderer sr, float size, Vector3 drift, float lifetime)
        {
            float elapsed = 0;
            Color c = sr.color;
            Vector3 pos = t.localPosition;
            t.localScale = Vector3.zero;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / lifetime;
                
                pos += drift * Time.unscaledDeltaTime;
                t.localPosition = pos;
                
                float scalePop = Mathf.Sin(p * Mathf.PI);
                t.localScale = Vector3.one * (size * scalePop);
                
                yield return null;
            }
            Destroy(t.gameObject);
        }

        private IEnumerator AnimateScaleAndAlpha(Transform t, SpriteRenderer sr, float startScale, float endScale, float startAlpha, float endAlpha, float dur)
        {
            float elapsed = 0;
            Color c = sr.color;
            while(elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                t.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, p);
                sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(startAlpha, endAlpha, p));
                yield return null;
            }
            Destroy(t.gameObject);
        }
    }
}
