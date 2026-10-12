using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class PhaseNineMilestoneWallSetup
    {
        [MenuItem("Tools/Geometry Dash/Phase 9: Strict Milestone System")]
        public static void RunPhaseNine()
        {
            // Create essential Sprites if they don't exist
            CreateSolidSprite("Assets/Sprites/MilestoneSquare.png", 16, 16, Color.white);
            CreateCircleSprite("Assets/Sprites/MilestoneCircle.png", 64);
            CreateRaySprite("Assets/Sprites/MilestoneRay.png", 64, 256);
            
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneSquare.png");
            Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneCircle.png");
            Sprite ray = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneRay.png");

            // 1. Build Panel Prefab
            GameObject panelPrefab = BuildResultPanelPrefab();

            // 2. Build VFX Prefab
            GameObject vfxPrefab = BuildVFXPrefab(square, circle, ray);

            // 3. Build Wall Prefab
            GameObject wallPrefab = BuildFinishWallPrefab();

            // 4. Update GameManager in TestScene
            UpdateGameManager(panelPrefab, vfxPrefab);

            // 5. Inject Wall into Levels 04, 08, 12
            InjectWallIntoLevels(wallPrefab);
            
            AssetDatabase.SaveAssets();
            Debug.Log("[Phase 9] Successfully created and injected Milestone Wall System!");
        }

        private static void CreateSolidSprite(string path, int w, int h, Color c)
        {
            if (File.Exists(path)) return;
            if (!Directory.Exists("Assets/Sprites")) Directory.CreateDirectory("Assets/Sprites");
            Texture2D t = new Texture2D(w, h);
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) t.SetPixel(x, y, c);
            t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG());
            AssetDatabase.Refresh();
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }

        private static void CreateCircleSprite(string path, int s)
        {
            if (File.Exists(path)) return;
            Texture2D t = new Texture2D(s, s);
            for(int y=0;y<s;y++) for(int x=0;x<s;x++) {
                float dist = Vector2.Distance(new Vector2(s/2, s/2), new Vector2(x, y));
                t.SetPixel(x, y, dist < s/2 ? Color.white : Color.clear);
            }
            t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG());
            AssetDatabase.Refresh();
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }

        private static void CreateRaySprite(string path, int w, int h)
        {
            if (File.Exists(path)) return;
            Texture2D t = new Texture2D(w, h);
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                float widthAtY = Mathf.Lerp(w/2f, 0f, y/(float)h);
                t.SetPixel(x, y, Mathf.Abs(x - w/2f) <= widthAtY ? Color.white : Color.clear);
            }
            t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG());
            AssetDatabase.Refresh();
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.SaveAndReimport();
        }

        private static GameObject BuildResultPanelPrefab()
        {
            GameObject canvasObj = new GameObject("MilestoneResultPanel");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Main Card
            GameObject panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(canvasObj.transform, false);
            RectTransform prt = panelRoot.AddComponent<RectTransform>();
            prt.sizeDelta = new Vector2(500, 600);
            
            // Outer Outline (Dark)
            GameObject outline = new GameObject("Outline");
            outline.transform.SetParent(panelRoot.transform, false);
            RectTransform outRt = outline.AddComponent<RectTransform>();
            outRt.anchorMin = Vector2.zero; outRt.anchorMax = Vector2.one;
            outRt.offsetMin = new Vector2(-10, -10); outRt.offsetMax = new Vector2(10, 10);
            outline.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f);

            // White Border
            GameObject white = new GameObject("White");
            white.transform.SetParent(panelRoot.transform, false);
            RectTransform wRt = white.AddComponent<RectTransform>();
            wRt.anchorMin = Vector2.zero; wRt.anchorMax = Vector2.one;
            wRt.offsetMin = new Vector2(-5, -5); wRt.offsetMax = new Vector2(5, 5);
            white.AddComponent<Image>().color = Color.white;

            // Green Frame
            GameObject green = new GameObject("GreenFrame");
            green.transform.SetParent(panelRoot.transform, false);
            RectTransform gRt = green.AddComponent<RectTransform>();
            gRt.anchorMin = Vector2.zero; gRt.anchorMax = Vector2.one;
            gRt.offsetMin = Vector2.zero; gRt.offsetMax = Vector2.zero;
            green.AddComponent<Image>().color = new Color(0.2f, 0.9f, 0.1f);

            // Dark Interior
            GameObject interior = new GameObject("Interior");
            interior.transform.SetParent(panelRoot.transform, false);
            RectTransform iRt = interior.AddComponent<RectTransform>();
            iRt.anchorMin = Vector2.zero; iRt.anchorMax = Vector2.one;
            iRt.offsetMin = new Vector2(25, 25); iRt.offsetMax = new Vector2(-25, -25);
            interior.AddComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f);

            // Level Complete Header
            GameObject header = new GameObject("Header");
            header.transform.SetParent(panelRoot.transform, false);
            TextMeshProUGUI hTxt = header.AddComponent<TextMeshProUGUI>();
            hTxt.text = "LEVEL COMPLETE!";
            hTxt.alignment = TextAlignmentOptions.Center;
            hTxt.fontStyle = FontStyles.Bold;
            hTxt.fontSize = 50;
            hTxt.color = new Color(0.3f, 1f, 0.2f);
            hTxt.outlineWidth = 0.2f;
            hTxt.outlineColor = Color.black;
            RectTransform hRt = header.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0, 1); hRt.anchorMax = new Vector2(1, 1);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.anchoredPosition = new Vector2(0, -50);
            hRt.sizeDelta = new Vector2(0, 100);

            // Attempts Label
            GameObject attemptsObj = new GameObject("AttemptsText");
            attemptsObj.transform.SetParent(panelRoot.transform, false);
            TextMeshProUGUI aTxt = attemptsObj.AddComponent<TextMeshProUGUI>();
            aTxt.text = "ATTEMPTS: 1";
            aTxt.alignment = TextAlignmentOptions.Center;
            aTxt.fontSize = 40;
            aTxt.color = Color.white;
            RectTransform aRt = attemptsObj.GetComponent<RectTransform>();
            aRt.anchorMin = new Vector2(0, 0.5f); aRt.anchorMax = new Vector2(1, 0.5f);
            aRt.anchoredPosition = new Vector2(0, 50);
            aRt.sizeDelta = new Vector2(0, 80);

            // Buttons Holder
            GameObject btnHolder = new GameObject("Buttons");
            btnHolder.transform.SetParent(panelRoot.transform, false);
            RectTransform bRt = btnHolder.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0, 0); bRt.anchorMax = new Vector2(1, 0);
            bRt.pivot = new Vector2(0.5f, 0f);
            bRt.anchoredPosition = new Vector2(0, 60);
            bRt.sizeDelta = new Vector2(0, 120);

            // 1. Replay (Left)
            Button replayBtn = CreateButton(btnHolder.transform, "ReplayBtn", -150, "REPLAY", new Color(0.2f, 0.8f, 1f));
            // 2. Next (Center)
            Button nextBtn = CreateButton(btnHolder.transform, "NextBtn", 0, "NEXT", new Color(1f, 0.7f, 0f));
            nextBtn.transform.localScale = Vector3.one * 1.2f; // Make it bigger
            // 3. Menu (Right)
            Button menuBtn = CreateButton(btnHolder.transform, "MenuBtn", 150, "MENU", new Color(0.7f, 0.7f, 0.7f));

            MilestoneResultPanel panel = canvasObj.AddComponent<MilestoneResultPanel>();
            panel.panelRoot = prt;
            panel.replayBtn = replayBtn;
            panel.nextBtn = nextBtn;
            panel.menuBtn = menuBtn;
            panel.attemptsText = aTxt;

            if (!Directory.Exists("Assets/Prefabs/UI")) Directory.CreateDirectory("Assets/Prefabs/UI");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/UI/MilestoneResultPanel.prefab");
            Object.DestroyImmediate(canvasObj);
            return prefab;
        }

        private static Button CreateButton(Transform parent, string name, float xPos, string text, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = new Vector2(xPos, 0);
            
            Image img = obj.AddComponent<Image>();
            img.color = color;
            
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 24;
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            return obj.AddComponent<Button>();
        }

        private static GameObject BuildVFXPrefab(Sprite square, Sprite circle, Sprite ray)
        {
            GameObject vfxRoot = new GameObject("MilestoneCompletionVFX");
            
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
            tmp.fontSize = 120;
            tmp.color = new Color(0.3f, 1f, 0.2f);
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = Color.black;
            tmp.gameObject.SetActive(false);
            
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            MilestoneCompletionVFX vfxController = vfxRoot.AddComponent<MilestoneCompletionVFX>();
            vfxController.worldCanvas = worldCanvas.transform;
            vfxController.screenCanvas = screenCanvasObj.transform;
            vfxController.completeText = tmp;
            vfxController.squareSprite = square;
            vfxController.circleSprite = circle;
            vfxController.raySprite = ray;

            if (!Directory.Exists("Assets/Prefabs/VFX")) Directory.CreateDirectory("Assets/Prefabs/VFX");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(vfxRoot, "Assets/Prefabs/VFX/MilestoneCompletionVFX.prefab");
            Object.DestroyImmediate(vfxRoot);
            return prefab;
        }

        private static GameObject BuildFinishWallPrefab()
        {
            GameObject root = new GameObject("MilestoneFinishWall");
            
            // Visual Wall
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "WallVisual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = new Vector3(1f, 5f, 0f);
            visual.transform.localScale = new Vector3(2f, 20f, 1f);
            Object.DestroyImmediate(visual.GetComponent<BoxCollider>()); // only visual
            
            // Trigger
            GameObject trigger = new GameObject("CompletionTrigger");
            trigger.transform.SetParent(root.transform);
            trigger.transform.localPosition = new Vector3(0f, 5f, 0f);
            BoxCollider2D box = trigger.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1f, 20f);
            trigger.AddComponent<MilestoneFinishTrigger>();
            
            if (!Directory.Exists("Assets/Prefabs/Environment")) Directory.CreateDirectory("Assets/Prefabs/Environment");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Environment/MilestoneFinishWall.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void UpdateGameManager(GameObject panelPrefab, GameObject vfxPrefab)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
            
            GameObject gm = GameObject.Find("GameManager");
            if (gm == null) gm = new GameObject("GameManager");
            
            // Remove old broken component
            var oldCtrl = gm.GetComponent<MilestoneLevelCompleteController>();
            if (oldCtrl != null) Object.DestroyImmediate(oldCtrl);

            var dir = gm.GetComponent<MilestoneCompletionDirector>();
            if (dir == null) dir = gm.AddComponent<MilestoneCompletionDirector>();
            
            dir.vfxPrefab = vfxPrefab.GetComponent<MilestoneCompletionVFX>();
            dir.panelPrefab = panelPrefab.GetComponent<MilestoneResultPanel>();

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }

        private static void InjectWallIntoLevels(GameObject wallPrefab)
        {
            int[] milestoneLevels = { 4, 8, 12 };
            foreach (int i in milestoneLevels)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:00}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                
                // Find existing EndAnchor to position the wall slightly before it
                Transform endAnchor = instance.transform.Find("EndAnchor");
                if (endAnchor != null)
                {
                    // Remove any old MilestoneFinish if it exists
                    Transform oldWall = instance.transform.Find("MilestoneFinish");
                    if (oldWall != null) Object.DestroyImmediate(oldWall.gameObject);

                    GameObject wallInstance = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
                    wallInstance.name = "MilestoneFinish";
                    wallInstance.transform.SetParent(instance.transform, false);
                    
                    // Place it exactly AT the EndAnchor minus a little offset so it acts as the new visual end
                    // Place it before the normal LevelEndTrigger (which is at EndAnchor.x - 5f).
                    wallInstance.transform.localPosition = new Vector3(endAnchor.localPosition.x - 7f, 0f, 0f);
                }

                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
            }
        }
    }
}
