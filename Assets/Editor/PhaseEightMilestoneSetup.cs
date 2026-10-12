using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;

namespace GeometryDash.Editor
{
    public static class PhaseEightMilestoneSetup
    {
        [MenuItem("Tools/Geometry Dash/Phase 8: Inject Milestone VFX")]
        public static void InjectMilestoneVFX()
        {
            if (!System.IO.Directory.Exists("Assets/Sprites"))
                System.IO.Directory.CreateDirectory("Assets/Sprites");

            // 1. Create the Sprite for Particles
            Texture2D tex = new Texture2D(16, 16);
            for(int y=0; y<16; y++) for(int x=0; x<16; x++) tex.SetPixel(x, y, Color.white);
            tex.Apply();
            byte[] texBytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes("Assets/Sprites/MilestoneSquare.png", texBytes);
            AssetDatabase.Refresh();
            
            TextureImporter ti = AssetImporter.GetAtPath("Assets/Sprites/MilestoneSquare.png") as TextureImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
            
            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneSquare.png");

            // Create Circle Sprite
            Texture2D circleTex = new Texture2D(64, 64);
            for(int y=0; y<64; y++) {
                for(int x=0; x<64; x++) {
                    float dist = Vector2.Distance(new Vector2(32, 32), new Vector2(x, y));
                    circleTex.SetPixel(x, y, dist < 32f ? Color.white : Color.clear);
                }
            }
            circleTex.Apply();
            System.IO.File.WriteAllBytes("Assets/Sprites/MilestoneCircle.png", circleTex.EncodeToPNG());
            AssetDatabase.Refresh();
            
            TextureImporter ti2 = AssetImporter.GetAtPath("Assets/Sprites/MilestoneCircle.png") as TextureImporter;
            ti2.textureType = TextureImporterType.Sprite;
            ti2.SaveAndReimport();
            
            Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneCircle.png");

            // Create Ray Sprite
            Texture2D rayTex = new Texture2D(64, 256);
            for(int y=0; y<256; y++) {
                for(int x=0; x<64; x++) {
                    float widthAtY = Mathf.Lerp(32f, 0f, y / 256f);
                    float distFromCenter = Mathf.Abs(x - 32f);
                    rayTex.SetPixel(x, y, distFromCenter <= widthAtY ? Color.white : Color.clear);
                }
            }
            rayTex.Apply();
            System.IO.File.WriteAllBytes("Assets/Sprites/MilestoneRay.png", rayTex.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter ti3 = AssetImporter.GetAtPath("Assets/Sprites/MilestoneRay.png") as TextureImporter;
            ti3.textureType = TextureImporterType.Sprite;
            ti3.SaveAndReimport();

            Sprite raySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneRay.png");

            // 2. Create the VFX Prefab
            GameObject vfxRoot = new GameObject("MilestoneLevelCompleteVFX");
            
            GameObject worldCanvas = new GameObject("WorldCanvas");
            worldCanvas.transform.SetParent(vfxRoot.transform);
            worldCanvas.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            
            GameObject screenCanvasObj = new GameObject("ScreenCanvas");
            screenCanvasObj.transform.SetParent(vfxRoot.transform);
            Canvas screenCanvas = screenCanvasObj.AddComponent<Canvas>();
            screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screenCanvas.sortingOrder = 100;
            screenCanvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject textObj = new GameObject("CompleteText");
            textObj.transform.SetParent(screenCanvasObj.transform);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "LEVEL COMPLETE!";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.fontStyle = FontStyles.Bold;
            tmp.gameObject.SetActive(false);
            
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            GeometryDash.Level.LevelCompleteVFXController vfxController = vfxRoot.AddComponent<GeometryDash.Level.LevelCompleteVFXController>();
            vfxController.worldCanvas = worldCanvas.transform;
            vfxController.screenCanvas = screenCanvasObj.transform;
            vfxController.completeText = tmp;
            vfxController.squareSprite = squareSprite;
            vfxController.circleSprite = circleSprite;
            vfxController.raySprite = raySprite;

            if (!System.IO.Directory.Exists("Assets/Prefabs/VFX"))
                System.IO.Directory.CreateDirectory("Assets/Prefabs/VFX");

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(vfxRoot, "Assets/Prefabs/VFX/MilestoneLevelCompleteVFX.prefab");
            Object.DestroyImmediate(vfxRoot);

            // 3. Inject into TestScene
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
            
            GameObject gm = GameObject.Find("GameManager");
            if (gm == null)
            {
                gm = new GameObject("GameManager");
            }
            
            GeometryDash.Level.MilestoneLevelCompleteController ctrl = gm.GetComponent<GeometryDash.Level.MilestoneLevelCompleteController>();
            if (ctrl == null)
            {
                ctrl = gm.AddComponent<GeometryDash.Level.MilestoneLevelCompleteController>();
            }
            ctrl.VFXPrefab = prefab;

            EditorSceneManager.SaveScene(scene);
            
            Debug.Log("[Phase 8] Successfully injected Milestone VFX Controller into TestScene!");
        }
    }
}
