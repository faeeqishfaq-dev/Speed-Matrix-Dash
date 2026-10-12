using UnityEngine;
using System.Collections;

namespace GeometryDash.Player
{
    public class PlayerRespawnVFX : MonoBehaviour
    {
        private Transform visualTransform;

        public void PlayRespawnVFX(Transform playerVisual)
        {
            this.visualTransform = playerVisual;
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            // 1. Blink
            if (visualTransform != null)
            {
                StartCoroutine(BlinkRoutine());
            }

            // 2. Spawn 2-3 thin circular outline rings overlapping in time
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(RingPulse());
                yield return new WaitForSeconds(0.1f);
            }

            // 3. Ground pixels
            SpawnGroundPixels();
        }

        private IEnumerator BlinkRoutine()
        {
            // The Cube flickers invisibly
            // Cube appears -> invisible -> visible -> invisible -> visible
            // Over ~0.25s
            if (visualTransform == null) yield break;
            
            SpriteRenderer sr = visualTransform.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            sr.enabled = true;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.enabled = true;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(0.05f);
            if (sr != null) sr.enabled = true;
        }

        private IEnumerator RingPulse()
        {
            // Draw a LineRenderer circle
            GameObject ring = new GameObject("RespawnRing");
            // Parent to the player root so it moves with the player perfectly
            ring.transform.SetParent(this.transform);
            ring.transform.localPosition = Vector3.zero;
            LineRenderer lr = ring.AddComponent<LineRenderer>();
            
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 32;
            
            float currentRadius = 0.1f;
            SetCirclePoints(lr, currentRadius);
            
            lr.startWidth = 0.04f;
            lr.endWidth = 0.04f;
            
            lr.material = new Material(Shader.Find("Sprites/Default"));
            Color ringColor = new Color(0.7f, 1f, 1f, 0.8f);
            lr.startColor = ringColor;
            lr.endColor = ringColor;

            float t = 0;
            float duration = 0.4f;
            float maxRadius = 2.5f; // 4-5 widths diameter = ~2.5 radius (Cube width is 1)

            while (t < duration)
            {
                t += Time.deltaTime;
                float pct = t / duration;
                
                // Expand fast out
                currentRadius = Mathf.Lerp(0.1f, maxRadius, Mathf.Pow(pct, 0.5f));
                SetCirclePoints(lr, currentRadius);
                
                // Fade out
                float alpha = Mathf.Lerp(0.8f, 0f, pct);
                Color c = new Color(ringColor.r, ringColor.g, ringColor.b, alpha);
                lr.startColor = c;
                lr.endColor = c;
                
                yield return null;
            }
            
            Destroy(ring);
        }

        private void SetCirclePoints(LineRenderer lr, float radius)
        {
            for (int i = 0; i < 32; i++)
            {
                float angle = (i / 32f) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
            }
        }

        private void SpawnGroundPixels()
        {
            // Tiny square particles near bottom
            for (int i = 0; i < 12; i++)
            {
                GameObject p = new GameObject("GroundPixel");
                p.transform.SetParent(this.transform);
                p.transform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, -0.2f), 0);
                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = VFXHelper.GetSquareSprite();
                sr.color = new Color(0.7f, 1f, 1f, 0.8f);
                p.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);
                
                // Move backward horizontally
                Vector3 vel = new Vector3(Random.Range(-2f, -0.2f), Random.Range(0f, 0.5f), 0);
                StartCoroutine(ParticleRoutine(p, vel, 0.35f));
            }
        }

        private IEnumerator ParticleRoutine(GameObject p, Vector3 vel, float duration)
        {
            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                p.transform.localPosition += vel * Time.deltaTime;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - (t/duration));
                yield return null;
            }
            Destroy(p);
        }
    }
}
