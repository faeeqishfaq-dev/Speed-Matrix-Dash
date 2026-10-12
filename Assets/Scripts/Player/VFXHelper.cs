using UnityEngine;

namespace GeometryDash.Player
{
    public static class VFXHelper
    {
        private static Sprite cachedSquare;
        private static Sprite cachedCircle;

        public static Sprite GetSquareSprite()
        {
            if (cachedSquare == null)
            {
                Texture2D tex = new Texture2D(16, 16);
                Color[] colors = new Color[16 * 16];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                cachedSquare = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            }
            return cachedSquare;
        }

        public static Sprite GetCircleSprite()
        {
            if (cachedCircle == null)
            {
                int res = 64;
                Texture2D tex = new Texture2D(res, res);
                Color[] colors = new Color[res * res];
                Vector2 center = new Vector2(res / 2f, res / 2f);
                float radius = res / 2f;
                for (int y = 0; y < res; y++)
                {
                    for (int x = 0; x < res; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center);
                        // Anti-aliased circle
                        float alpha = Mathf.Clamp01(radius - dist);
                        colors[y * res + x] = new Color(1, 1, 1, alpha);
                    }
                }
                tex.SetPixels(colors);
                tex.Apply();
                cachedCircle = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            }
            return cachedCircle;
        }
    }
}
