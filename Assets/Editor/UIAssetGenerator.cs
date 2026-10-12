using UnityEngine;
using UnityEditor;
using System.IO;

namespace GeometryDash.Editor
{
    public static class UIAssetGenerator
    {
        private static string outPath = "Assets/UI/Sprites";

        [MenuItem("Tools/Geometry Dash/1. Generate UI Assets")]
        public static void GenerateUIAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
            if (!AssetDatabase.IsValidFolder(outPath)) AssetDatabase.CreateFolder("Assets/UI", "Sprites");

            GenerateCircle("Button_Round_Green", new Color(0.3f, 1f, 0.2f), true, 128);
            GenerateCircle("Button_Round_Dark", new Color(0.1f, 0.3f, 0.5f), true, 128);
            GenerateTriangle("Icon_Play");
            GenerateGear("Icon_Gear");
            GenerateList("Icon_List");
            GenerateArrow("Icon_Restart");
            GenerateDiamond("Icon_Diamond");
            GenerateCubeFace("Icon_Cube");
            
            GeneratePanel("Panel_Settings", new Color(0.05f, 0.1f, 0.3f), new Color(0.3f, 1f, 0.2f), new Color(0f, 0.8f, 1f));
            GeneratePanel("Panel_Overlay", new Color(0f, 0f, 0f, 0.7f), Color.clear, Color.clear, 128);
            GenerateWideButton("Button_Wide_Green", new Color(0.3f, 1f, 0.2f));
            
            GenerateSliderTrack("Slider_Track");
            GenerateCircle("Slider_Knob", new Color(1f, 0.8f, 0.1f), false, 64);

            GenerateProgressBar("Progress_Frame", Color.black, Color.white);
            GenerateProgressBar("Progress_Fill", new Color(0.3f, 1f, 0.2f), Color.clear);
            
            GenerateLoadingBarFill("Loading_Fill");

            GenerateBackground("BG_BlueGeo");

            AssetDatabase.Refresh();
            Debug.Log("[UI] All UI Assets Generated Successfully!");
        }

        private static void SaveTex(Texture2D tex, string name, bool isSprite = true, Vector4 border = default)
        {
            string p = $"{outPath}/{name}.png";
            File.WriteAllBytes(p, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                if (border != default)
                {
                    importer.spriteBorder = border;
                }
                importer.SaveAndReimport();
            }
        }

        private static void GenerateCircle(string name, Color mainCol, bool doubleBorder, int size)
        {
            Texture2D t = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    Color c = Color.clear;
                    if (d < r)
                    {
                        if (doubleBorder && d > r - 6) c = Color.white;
                        else if (d > r - 12) c = Color.black;
                        else c = mainCol;
                        
                        // Shadow/highlight
                        if (d <= r - 12)
                        {
                            if (y < size * 0.3f) c *= 0.6f;
                            if (y > size * 0.7f) c = Color.Lerp(c, Color.white, 0.3f);
                        }
                    }
                    t.SetPixel(x, y, c);
                }
            }
            t.Apply();
            SaveTex(t, name);
        }

        private static void GenerateTriangle(string name)
        {
            int s = 128;
            Texture2D t = new Texture2D(s, s);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    t.SetPixel(x, y, Color.clear);
                    // Right facing triangle
                    float pY = Mathf.Abs(y - s/2f) / (s/2f);
                    float boundX = (1f - pY) * (s - 20f);
                    if (x > 20 && x < boundX)
                    {
                        if (x < 30 || x > boundX - 10 || pY > 0.85f) t.SetPixel(x, y, Color.black);
                        else t.SetPixel(x, y, new Color(1f, 0.9f, 0.1f));
                    }
                }
            }
            t.Apply();
            SaveTex(t, name);
        }

        private static void GenerateGear(string name)
        {
            int s = 128;
            Texture2D t = new Texture2D(s, s);
            Vector2 c = new Vector2(s/2f, s/2f);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x,y), c);
                    float a = Mathf.Atan2(y - c.y, x - c.x);
                    float teeth = Mathf.Sin(a * 8f);
                    float maxR = 50f + (teeth > 0 ? 10f : 0f);
                    
                    if (d < maxR && d > 20f) t.SetPixel(x, y, Color.white);
                    else t.SetPixel(x, y, Color.clear);
                }
            }
            t.Apply();
            SaveTex(t, name);
        }

        private static void GenerateList(string name)
        {
            int s = 128;
            Texture2D t = new Texture2D(s, s);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    t.SetPixel(x, y, Color.clear);
                    if (x > 20 && x < 108)
                    {
                        if ((y > 20 && y < 35) || (y > 55 && y < 70) || (y > 90 && y < 105))
                            t.SetPixel(x, y, Color.white);
                    }
                }
            }
            t.Apply();
            SaveTex(t, name);
        }

        private static void GenerateArrow(string name)
        {
            int s = 128;
            Texture2D t = new Texture2D(s, s);
            Vector2 c = new Vector2(s/2f, s/2f);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x,y), c);
                    t.SetPixel(x, y, Color.clear);
                    if (d > 35 && d < 55)
                    {
                        if (x > s/2f || y > s/2f) t.SetPixel(x, y, Color.white); // 3/4 circle
                    }
                    // Arrow head
                    if (x > 64 && x < 94 && y > 50 && y < 80 && x-y > 10) t.SetPixel(x,y, Color.white);
                }
            }
            t.Apply();
            SaveTex(t, name);
        }

        private static void GenerateDiamond(string name)
        {
            int s = 128;
            Texture2D t = new Texture2D(s, s);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = Mathf.Abs(x - s/2f);
                    float dy = Mathf.Abs(y - s/2f);
                    if (dx + dy < 50) t.SetPixel(x,y, Color.white);
                    else t.SetPixel(x,y, Color.clear);
                }
            }
            t.Apply();
            SaveTex(t, name);
        }
        
        private static void GenerateCubeFace(string name)
        {
            int s = 128;
            Texture2D t = new Texture2D(s, s);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    if (x > 16 && x < 112 && y > 16 && y < 112)
                    {
                        if (x > 32 && x < 48 && y > 64 && y < 80) t.SetPixel(x,y, Color.black); // eye
                        else if (x > 80 && x < 96 && y > 64 && y < 80) t.SetPixel(x,y, Color.black); // eye
                        else if (x > 32 && x < 96 && y > 32 && y < 48) t.SetPixel(x,y, Color.black); // mouth
                        else t.SetPixel(x,y, new Color(1f, 0.8f, 0f));
                    }
                    else t.SetPixel(x,y, Color.clear);
                }
            }
            t.Apply();
            SaveTex(t, name);
        }

        private static void GeneratePanel(string name, Color bg, Color border, Color corner, int size = 256)
        {
            Texture2D t = new Texture2D(size, size);
            int bT = 8; // white border
            int bB = 16; // black border
            int bF = 32; // frame border
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    t.SetPixel(x, y, Color.clear);
                    
                    if (bg.a == 0 && corner.a == 0 && border.a == 0) // simple overlay
                    {
                        t.SetPixel(x,y, bg);
                        continue;
                    }

                    if (x < bT || x > size-bT || y < bT || y > size-bT) t.SetPixel(x,y, Color.white);
                    else if (x < bB || x > size-bB || y < bB || y > size-bB) t.SetPixel(x,y, Color.black);
                    else if (x < bF || x > size-bF || y < bF || y > size-bF) t.SetPixel(x,y, border);
                    else t.SetPixel(x,y, bg);
                    
                    // Cyan corners
                    if (corner.a > 0)
                    {
                        if ((x < 48 || x > size-48) && (y < 48 || y > size-48))
                        {
                            if (x > bB && x < size-bB && y > bB && y < size-bB)
                                t.SetPixel(x, y, corner);
                        }
                    }
                }
            }
            t.Apply();
            SaveTex(t, name, true, new Vector4(bF+16, bF+16, bF+16, bF+16)); // 9-slice
        }

        private static void GenerateWideButton(string name, Color bg)
        {
            int w = 256;
            int h = 64;
            Texture2D t = new Texture2D(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < 4 || x > w-4 || y < 4 || y > h-4) t.SetPixel(x,y, Color.black);
                    else
                    {
                        Color c = bg;
                        if (y < h*0.3f) c *= 0.6f;
                        t.SetPixel(x,y, c);
                    }
                }
            }
            t.Apply();
            SaveTex(t, name, true, new Vector4(16, 16, 16, 16));
        }

        private static void GenerateSliderTrack(string name)
        {
            int w = 256;
            int h = 32;
            Texture2D t = new Texture2D(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (y < 4 || y > h-4 || x < 4 || x > w-4) t.SetPixel(x,y, new Color(1f, 0.8f, 0.1f)); // gold
                    else t.SetPixel(x,y, new Color(0f, 0.8f, 1f)); // cyan
                }
            }
            t.Apply();
            SaveTex(t, name, true, new Vector4(16, 16, 16, 16));
        }

        private static void GenerateProgressBar(string name, Color bg, Color border)
        {
            int w = 256;
            int h = 32;
            Texture2D t = new Texture2D(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (border.a > 0 && (y < 4 || y > h-4 || x < 4 || x > w-4)) t.SetPixel(x,y, border);
                    else t.SetPixel(x,y, bg);
                }
            }
            t.Apply();
            SaveTex(t, name, true, new Vector4(8, 8, 8, 8));
        }

        private static void GenerateLoadingBarFill(string name)
        {
            int w = 256;
            int h = 32;
            Texture2D t = new Texture2D(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = new Color(0f, 0.8f, 1f);
                    if ((x + y) % 32 < 16) c = Color.Lerp(c, Color.white, 0.5f); // diagonal stripes
                    t.SetPixel(x,y, c);
                }
            }
            t.Apply();
            SaveTex(t, name, true, new Vector4(8, 8, 8, 8));
        }

        private static void GenerateBackground(string name)
        {
            int s = 512;
            Texture2D t = new Texture2D(s, s);
            Color baseBlue = new Color(0.05f, 0.2f, 0.5f);
            Color darkBlue = new Color(0.02f, 0.1f, 0.3f);
            Color lightBlue = new Color(0.1f, 0.3f, 0.6f);

            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    t.SetPixel(x, y, baseBlue);
                    
                    // Simple geometric pattern
                    if (x % 128 < 4 || y % 128 < 4) t.SetPixel(x,y, darkBlue);
                    else if ((x/128 + y/128) % 2 == 0) t.SetPixel(x,y, lightBlue);
                }
            }
            t.Apply();
            
            string p = $"{outPath}/{name}.png";
            File.WriteAllBytes(p, t.EncodeToPNG());
            AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            
            TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }
    }
}
