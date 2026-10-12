using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace GeometryDash.Level
{
    public class MilestoneCompletionVFX : MonoBehaviour
    {
        [Header("Scene References")]
        public Transform worldCanvas;
        public Transform screenCanvas;
        public TextMeshProUGUI completeText;
        public Sprite circleSprite;
        public Sprite squareSprite;
        public Sprite raySprite;
        public Sprite ringSprite;

        private MilestoneCompletionDirector config;

        private void Awake()
        {
            if (ringSprite == null)
            {
#if UNITY_EDITOR
                ringSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneRing.png");
#endif
            }
            if (ringSprite == null) ringSprite = circleSprite;
        }

        public void PlaySequence(MilestoneCompletionDirector config)
        {
            this.config = config;
            StartCoroutine(SequenceRoutine());
        }

        private IEnumerator SequenceRoutine()
        {
            // PHASE 1 — IMPACT CONTACT / WAVE (T = 0.00s)
            // FIRST: a wave must come out before big beam lights!
            SpawnContactWave();

            // Short pause so wave clearly reads first
            yield return new WaitForSecondsRealtime(config.beamStartDelay);

            // PHASE 2 — BEAM LIGHTS (T = 0.25s to 4.2s)
            // Beam lights are permanently attached to the finish wall and driven by MilestoneWallVFXController

            // PHASE 3 — CELEBRATION PARTICLES BLAST (T = 0.30s to 4.5s)
            yield return new WaitForSecondsRealtime(0.05f);
            SpawnCelebrationBlast();
            StartCoroutine(CelebrationSparkleStreamRoutine());

            // Secondary pulse wait at T = 1.0s to preserve exact timing
            yield return new WaitForSecondsRealtime(0.65f);

            // PHASE 4 — LEVEL COMPLETE TEXT (T = 1.80s)
            yield return new WaitForSecondsRealtime(config.TextDelay - config.beamStartDelay - 0.70f);
            ShowText();
            SpawnSecondaryBlast();
            SpawnCelebrationCircles();
            SpawnFloatingOrbs();

            // Sequence gracefully settles between 4.0s and 5.0s
            // (Panel will be shown at T = 5.0s by MilestoneCompletionDirector)
        }

        #region Phase 1: Contact Wave
        private void SpawnContactWave()
        {
            // 1. Primary Expanding Shockwave Pulse
            GameObject waveObj = new GameObject("ContactWave");
            waveObj.transform.SetParent(worldCanvas, false);
            waveObj.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = waveObj.AddComponent<SpriteRenderer>();
            sr.sprite = ringSprite != null ? ringSprite : circleSprite;
            sr.color = new Color(1f, 0.95f, 0.6f, 1f);
            sr.sortingOrder = 50;

            float maxScale = config.contactWaveScale;
            float dur = config.contactWaveDuration;
            StartCoroutine(AnimateShockwave(waveObj.transform, sr, 0f, maxScale, 1.0f, 0f, dur));

            // 2. Dual-Wave: Secondary concentric ring at T = 0.08s
            StartCoroutine(DelayedSecondaryWave(0.08f, maxScale * 0.75f, dur * 0.85f));

            // 3. Central bright impact flash
            GameObject flash = new GameObject("CenterImpactFlash");
            flash.transform.SetParent(worldCanvas, false);
            flash.transform.localPosition = Vector3.zero;
            SpriteRenderer flashSr = flash.AddComponent<SpriteRenderer>();
            flashSr.sprite = circleSprite;
            flashSr.color = Color.white;
            flashSr.sortingOrder = 51;
            StartCoroutine(AnimateScaleAndAlpha(flash.transform, flashSr, 0f, 12f, 1f, 0f, 0.22f));
        }

        private IEnumerator DelayedSecondaryWave(float delay, float maxScale, float dur)
        {
            yield return new WaitForSecondsRealtime(delay);
            GameObject waveObj = new GameObject("ContactWaveSecondary");
            waveObj.transform.SetParent(worldCanvas, false);
            waveObj.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = waveObj.AddComponent<SpriteRenderer>();
            sr.sprite = ringSprite != null ? ringSprite : circleSprite;
            sr.color = new Color(1f, 0.8f, 0.2f, 0.85f);
            sr.sortingOrder = 49;
            StartCoroutine(AnimateShockwave(waveObj.transform, sr, 0f, maxScale, 0.9f, 0f, dur));
        }

        private IEnumerator AnimateShockwave(Transform t, SpriteRenderer sr, float startScale, float endScale, float startAlpha, float endAlpha, float dur)
        {
            float elapsed = 0;
            Color c = sr.color;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                // Snappy ease-out
                float easeOut = 1f - Mathf.Pow(1f - p, 3f);
                t.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, easeOut);
                sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(startAlpha, endAlpha, p));
                yield return null;
            }
            Destroy(t.gameObject);
        }
        #endregion

        #region Phase 2: Beam Lights
        // Beam lights are permanent Editor objects under MilestoneFinish/LightBeamEmitter (Beam_01 - Beam_06)
        // animated directly by MilestoneWallVFXController. No runtime instantiation of beams.
        #endregion

        #region Phase 3: Blast & Celebration Particles
        private void SpawnCelebrationBlast()
        {
            int count = config.blastParticleCount;
            for (int i = 0; i < count; i++)
            {
                // Primary celebratory square particles fanning broadly across the screen
                SpawnSingleParticle(
                    config.blastParticleSpeed * Random.Range(0.6f, 1.4f),
                    config.blastParticleLifetime * Random.Range(0.7f, 1.25f),
                    isBurst: false
                );
            }
        }

        private IEnumerator CelebrationSparkleStreamRoutine()
        {
            // Continuously emits floating sparkle particles across T = 0.4s to 4.0s
            // to ensure the screen feels active and magical throughout the entire 5 seconds
            float elapsed = 0f;
            float totalStreamTime = 3.6f;
            float interval = 1f / Mathf.Max(1f, config.secondaryParticleRate);

            while (elapsed < totalStreamTime)
            {
                elapsed += interval;
                yield return new WaitForSecondsRealtime(interval);

                // Spawn floating sparkle particle
                SpawnFloatingSparkleParticle();
            }
        }

        private void SpawnSingleParticle(float speed, float lifetime, bool isBurst)
        {
            GameObject p = new GameObject("CelebrationParticle");
            p.transform.SetParent(worldCanvas, false);
            p.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;

            // Controlled golden yellow / warm gold / light orange / white-yellow palette
            Color[] palette = {
                new Color(1f, 0.98f, 0.77f), // white-yellow #FFF9C5
                new Color(1f, 0.95f, 0.25f), // lemon yellow #FFF23F
                new Color(1f, 0.83f, 0.16f), // golden yellow #FFD329
                new Color(1f, 0.76f, 0.16f), // warm gold #FFC229
                new Color(1f, 0.65f, 0.13f)  // light orange #FFA522
            };
            sr.color = palette[Random.Range(0, palette.Length)];
            sr.sortingOrder = 40;

            // Fan leftward across gameplay area (100 to 260 degrees)
            float angle = Random.Range(180f - 75f, 180f + 75f);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);

            float size = Random.Range(0.18f, 0.45f);
            float rotSpeed = Random.Range(-90f, 90f);

            StartCoroutine(AnimateCelebrationParticle(p.transform, sr, dir, speed, lifetime, size, rotSpeed));
        }

        private void SpawnFloatingSparkleParticle()
        {
            GameObject p = new GameObject("FloatingSparkle");
            p.transform.SetParent(worldCanvas, false);
            // Spawn along wall height
            p.transform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-8f, 8f), 0f);
            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = Random.value > 0.4f ? squareSprite : circleSprite;

            Color[] palette = {
                new Color(1f, 0.98f, 0.8f, 0.9f),
                new Color(1f, 0.88f, 0.3f, 0.85f),
                new Color(1f, 0.70f, 0.2f, 0.8f)
            };
            sr.color = palette[Random.Range(0, palette.Length)];
            sr.sortingOrder = 38;

            // Drifting gently leftward and floating upward/downward
            Vector3 drift = new Vector3(Random.Range(-2.5f, -6.5f), Random.Range(-1.2f, 1.8f), 0f);
            float lifetime = Random.Range(1.8f, 3.2f);
            float size = Random.Range(0.12f, 0.28f);

            StartCoroutine(AnimateFloatingSparkle(p.transform, sr, drift, lifetime, size));
        }

        private IEnumerator AnimateCelebrationParticle(Transform t, SpriteRenderer sr, Vector3 dir, float initialSpeed, float lifetime, float baseSize, float rotSpeed)
        {
            t.localScale = Vector3.one * baseSize;
            float elapsed = 0;
            Color c = sr.color;
            Vector3 pos = Vector3.zero;
            float speed = initialSpeed;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / lifetime;

                pos += dir * speed * Time.unscaledDeltaTime;
                // Smooth drag deceleration so particles fan across the screen and float
                speed = Mathf.Lerp(speed, 0.8f, Time.unscaledDeltaTime * 2.8f);

                t.localPosition = pos;
                t.Rotate(0, 0, rotSpeed * Time.unscaledDeltaTime);

                // Smooth fade and gentle shrink towards end
                float alpha = p < 0.7f ? 1.0f : Mathf.Lerp(1.0f, 0f, (p - 0.7f) / 0.3f);
                float sizeMult = Mathf.Lerp(1.0f, 0.45f, p);

                sr.color = new Color(c.r, c.g, c.b, c.a * alpha);
                t.localScale = Vector3.one * (baseSize * sizeMult);
                yield return null;
            }
            Destroy(t.gameObject);
        }

        private IEnumerator AnimateFloatingSparkle(Transform t, SpriteRenderer sr, Vector3 velocity, float lifetime, float size)
        {
            t.localScale = Vector3.zero;
            float elapsed = 0;
            Color c = sr.color;
            Vector3 pos = t.localPosition;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / lifetime;

                pos += velocity * Time.unscaledDeltaTime;
                t.localPosition = pos;

                // Pop in and fade out curve
                float pop = Mathf.Sin(p * Mathf.PI);
                t.localScale = Vector3.one * (size * pop);
                sr.color = new Color(c.r, c.g, c.b, c.a * pop);

                yield return null;
            }
            Destroy(t.gameObject);
        }
        #endregion

        #region Phase 4: Level Complete Text & Celebration Orbs
        private void ShowText()
        {
            if (completeText != null)
            {
                completeText.gameObject.SetActive(true);
                StartCoroutine(AnimateTextPop());
            }
        }

        private IEnumerator AnimateTextPop()
        {
            float dur = config.TextPopDuration;
            float elapsed = 0;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                float scale = 0f;
                if (p < 0.6f) scale = Mathf.Lerp(0.65f, 1.15f, p / 0.6f);
                else scale = Mathf.Lerp(1.15f, 1.0f, (p - 0.6f) / 0.4f);

                completeText.transform.localScale = Vector3.one * scale;
                completeText.alpha = Mathf.Clamp01(p * 2.2f);
                yield return null;
            }
            completeText.transform.localScale = Vector3.one;
        }

        private void SpawnSecondaryBlast()
        {
            if (completeText == null) return;
            GameObject blast = new GameObject("SecondaryBlast");
            blast.transform.SetParent(worldCanvas, false);
            blast.transform.position = completeText.transform.position; // behind text
            SpriteRenderer sr = blast.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = new Color(1f, 0.65f, 0.1f, 0.5f);
            sr.sortingOrder = 25;
            StartCoroutine(AnimateScaleAndAlpha(blast.transform, sr, 0f, 26f, 0.6f, 0f, 1.2f));
        }

        private void SpawnCelebrationCircles()
        {
            for (int i = 0; i < 6; i++)
            {
                SpawnOrb(Random.Range(3f, 8f), new Color(1f, 0.85f, 0.1f, 0.25f), Random.insideUnitCircle * 6f);
            }
        }

        private void SpawnFloatingOrbs()
        {
            for (int i = 0; i < 8; i++)
            {
                Color[] colors = { Color.white, Color.cyan, new Color(1f, 0.8f, 0.3f), new Color(1f, 0.6f, 0.9f) };
                Color c = colors[Random.Range(0, colors.Length)];
                c.a = 0.35f;
                SpawnOrb(Random.Range(2f, 5.5f), c, Random.insideUnitCircle * 10f);
            }
        }

        private void SpawnOrb(float size, Color c, Vector3 offset)
        {
            GameObject orb = new GameObject("CelebrationOrb");
            orb.transform.SetParent(worldCanvas, false);
            orb.transform.localPosition = offset;
            SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = c;
            sr.sortingOrder = 26;

            Vector3 drift = Random.insideUnitCircle.normalized * 2.2f;
            StartCoroutine(AnimateOrb(orb.transform, sr, size, drift, 2.5f));
        }

        private IEnumerator AnimateOrb(Transform t, SpriteRenderer sr, float size, Vector3 drift, float lifetime)
        {
            float elapsed = 0;
            Color c = sr.color;
            Vector3 pos = t.localPosition;

            while (elapsed < lifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / lifetime;

                pos += drift * Time.unscaledDeltaTime;
                t.localPosition = pos;

                float scalePop = Mathf.Sin(p * Mathf.PI);
                t.localScale = Vector3.one * (size * scalePop);
                sr.color = new Color(c.r, c.g, c.b, c.a * scalePop);
                yield return null;
            }
            Destroy(t.gameObject);
        }

        private IEnumerator AnimateScaleAndAlpha(Transform t, SpriteRenderer sr, float startScale, float endScale, float startAlpha, float endAlpha, float dur)
        {
            float elapsed = 0;
            Color c = sr.color;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / dur;
                t.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, p);
                sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(startAlpha, endAlpha, p));
                yield return null;
            }
            Destroy(t.gameObject);
        }
        #endregion
    }
}
