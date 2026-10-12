using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using GeometryDash.UI;
using UnityEngine.EventSystems;

namespace GeometryDash.Editor
{
    public static class PhaseThreeUISetup
    {
        [MenuItem("Tools/Geometry Dash/3. Run Phase 3 UI Setup")]
        public static void RunUISetup()
        {
            // 1. Generate Assets
            UIAssetGenerator.GenerateUIAssets();

            // 2. Build Loading Scene
            BuildLoadingScene();

            // 3. Build Main Menu Scene
            BuildMainMenuScene();

            // 4. Inject Pause Menu into TestScene
            InjectPauseMenu();

            // 5. Update Build Settings
            UpdateBuildSettings();

            Debug.Log("[Phase 3] Complete Professional UI Setup Finished!");
        }

        private static void BuildLoadingScene()
        {
            UnityEngine.SceneManagement.Scene s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Camera
            GameObject camObj = new GameObject("Main Camera");
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            
            // Canvas
            GameObject canvasObj = CreateCanvas("LoadingCanvas");
            
            // BG
            CreateImage(canvasObj, "Background", "BG_BlueGeo", new Color(1,1,1,1), true);

            // Title
            GameObject title = CreateText(canvasObj, "Title", "GEOMETRY JUMP", 120, new Color(0.2f, 1f, 0.2f), new Vector2(0, 150));
            AddOutline(title, Color.black, 6);
            AddOutline(title, Color.white, 3);
            
            // Controller
            GameObject controllerObj = new GameObject("LoadingScreenController");
            LoadingScreenController lsc = controllerObj.AddComponent<LoadingScreenController>();
            
            // Loading Bar
            GameObject bar = CreateProgressBar(canvasObj, "LoadingBar", new Vector2(0, -100), new Vector2(800, 40));
            lsc.GetType().GetField("loadingBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(lsc, bar.GetComponent<Slider>());

            // Tip
            GameObject tip = CreateText(canvasObj, "TipText", "TIP", 40, new Color(1f, 0.8f, 0f), new Vector2(0, -180));
            AddOutline(tip, Color.black, 3);
            lsc.GetType().GetField("tipText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(lsc, tip.GetComponent<Text>());

            CreateEventSystem();

            EditorSceneManager.SaveScene(s, "Assets/Scenes/LoadingScene.unity");
        }

        private static void BuildMainMenuScene()
        {
            UnityEngine.SceneManagement.Scene s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            GameObject camObj = new GameObject("Main Camera");
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            
            GameObject canvasObj = CreateCanvas("MainMenuCanvas");
            CreateImage(canvasObj, "Background", "BG_BlueGeo", new Color(1,1,1,1), true);

            GameObject title = CreateText(canvasObj, "Title", "GEOMETRY JUMP", 150, new Color(0.2f, 1f, 0.2f), new Vector2(0, 300));
            AddOutline(title, Color.black, 8);
            AddOutline(title, Color.white, 4);

            // Main Menu Controller
            GameObject controllerObj = new GameObject("MainMenuController");
            MainMenuController mmc = controllerObj.AddComponent<MainMenuController>();

            // Buttons
            GameObject btnPlay = CreateButton(canvasObj, "Btn_Play", "Button_Round_Green", new Vector2(0, 0), new Vector2(256, 256));
            CreateImage(btnPlay, "Icon", "Icon_Play", Color.white, false, new Vector2(120, 120));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlay.GetComponent<Button>().onClick, mmc.OnPlayClicked);

            GameObject btnSkins = CreateButton(canvasObj, "Btn_Skins", "Button_Round_Green", new Vector2(-300, 0), new Vector2(180, 180));
            CreateImage(btnSkins, "Icon", "Icon_Cube", Color.white, false, new Vector2(100, 100));

            GameObject btnLevels = CreateButton(canvasObj, "Btn_Levels", "Button_Round_Green", new Vector2(300, 0), new Vector2(180, 180));
            CreateImage(btnLevels, "Icon", "Icon_List", Color.white, false, new Vector2(100, 100));

            GameObject btnSettings = CreateButton(canvasObj, "Btn_Settings", "Button_Round_Green", new Vector2(0, -350), new Vector2(128, 128));
            CreateImage(btnSettings, "Icon", "Icon_Gear", Color.white, false, new Vector2(80, 80));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettings.GetComponent<Button>().onClick, mmc.OnSettingsClicked);

            // Settings Panel
            GameObject settingsPanel = CreateImage(canvasObj, "SettingsPanel", "Panel_Settings", Color.white, false, new Vector2(1200, 800));
            settingsPanel.SetActive(false); // Default hidden
            UIScaleAnimator animator = settingsPanel.AddComponent<UIScaleAnimator>();
            mmc.GetType().GetField("settingsPanelAnimator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(mmc, animator);

            CreateText(settingsPanel, "Header", "SETTINGS", 80, Color.white, new Vector2(0, 300));
            
            GameObject btnClose = CreateButton(settingsPanel, "Btn_Close", "Button_Round_Dark", new Vector2(-550, 350), new Vector2(100, 100));
            CreateText(btnClose, "X", "X", 60, Color.white, Vector2.zero);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnClose.GetComponent<Button>().onClick, mmc.OnSettingsClosed);

            // Audio settings
            GameObject audioObj = new GameObject("AudioSettingsController");
            AudioSettingsController asc = audioObj.AddComponent<AudioSettingsController>();

            GameObject musicSlider = CreateSlider(settingsPanel, "MusicSlider", new Vector2(0, -100));
            CreateText(settingsPanel, "MusicLabel", "MUSIC", 40, Color.white, new Vector2(0, -40));
            asc.GetType().GetField("musicSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(asc, musicSlider.GetComponent<Slider>());

            GameObject sfxSlider = CreateSlider(settingsPanel, "SFXSlider", new Vector2(0, -250));
            CreateText(settingsPanel, "SFXLabel", "SFX", 40, Color.white, new Vector2(0, -190));
            asc.GetType().GetField("sfxSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(asc, sfxSlider.GetComponent<Slider>());

            CreateEventSystem();

            EditorSceneManager.SaveScene(s, "Assets/Scenes/MainMenuScene.unity");
        }

        private static void InjectPauseMenu()
        {
            UnityEngine.SceneManagement.Scene s = EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
            
            // Create Canvas
            GameObject canvasObj = CreateCanvas("InGameUICanvas");
            
            // Audio Controller
            GameObject audioObj = new GameObject("AudioSettingsController");
            AudioSettingsController asc = audioObj.AddComponent<AudioSettingsController>();

            // Pause Controller
            GameObject pauseObj = new GameObject("PauseMenuController");
            PauseMenuController pmc = pauseObj.AddComponent<PauseMenuController>();

            // Top Right Settings Button
            GameObject btnPause = CreateButton(canvasObj, "Btn_Pause", "Button_Round_Green", new Vector2(-100, -100), new Vector2(128, 128));
            RectTransform btnPauseRT = btnPause.GetComponent<RectTransform>();
            btnPauseRT.anchorMin = new Vector2(1, 1);
            btnPauseRT.anchorMax = new Vector2(1, 1);
            CreateImage(btnPause, "Icon", "Icon_Gear", Color.white, false, new Vector2(80, 80));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPause.GetComponent<Button>().onClick, pmc.OnPauseClicked);

            // Overlay
            GameObject overlay = CreateImage(canvasObj, "PauseOverlay", "Panel_Overlay", Color.white, true);
            overlay.SetActive(false);
            pmc.GetType().GetField("pausePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pmc, overlay);

            UIScaleAnimator animator = overlay.AddComponent<UIScaleAnimator>();
            pmc.GetType().GetField("pauseAnimator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pmc, animator);

            CreateText(overlay, "Header", "NORMAL MODE", 80, Color.white, new Vector2(0, 300));
            CreateProgressBar(overlay, "LevelProgress", new Vector2(0, 200), new Vector2(800, 40));

            // 4 Buttons
            GameObject btnPractice = CreateButton(overlay, "Btn_Practice", "Button_Round_Green", new Vector2(-300, 0), new Vector2(180, 180));
            CreateImage(btnPractice, "Icon", "Icon_Diamond", Color.white, false, new Vector2(100, 100));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPractice.GetComponent<Button>().onClick, pmc.OnPracticeClicked);

            GameObject btnResume = CreateButton(overlay, "Btn_Resume", "Button_Round_Green", new Vector2(-100, 0), new Vector2(220, 220));
            CreateImage(btnResume, "Icon", "Icon_Play", Color.white, false, new Vector2(110, 110));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnResume.GetComponent<Button>().onClick, pmc.OnResumeClicked);

            GameObject btnMenu = CreateButton(overlay, "Btn_Menu", "Button_Round_Green", new Vector2(100, 0), new Vector2(180, 180));
            CreateImage(btnMenu, "Icon", "Icon_List", Color.white, false, new Vector2(100, 100));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnMenu.GetComponent<Button>().onClick, pmc.OnMainMenuClicked);

            GameObject btnRestart = CreateButton(overlay, "Btn_Restart", "Button_Round_Green", new Vector2(300, 0), new Vector2(180, 180));
            CreateImage(btnRestart, "Icon", "Icon_Restart", Color.white, false, new Vector2(100, 100));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnRestart.GetComponent<Button>().onClick, pmc.OnRestartClicked);

            // Audio Settings
            GameObject musicSlider = CreateSlider(overlay, "MusicSlider", new Vector2(-300, -250), new Vector2(400, 32));
            CreateText(overlay, "MusicLabel", "MUSIC", 40, Color.white, new Vector2(-300, -190));
            asc.GetType().GetField("musicSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(asc, musicSlider.GetComponent<Slider>());

            GameObject sfxSlider = CreateSlider(overlay, "SFXSlider", new Vector2(300, -250), new Vector2(400, 32));
            CreateText(overlay, "SFXLabel", "SFX", 40, Color.white, new Vector2(300, -190));
            asc.GetType().GetField("sfxSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(asc, sfxSlider.GetComponent<Slider>());

            CreateEventSystem();

            EditorSceneManager.SaveScene(s, "Assets/Scenes/TestScene.unity");
        }

        private static void UpdateBuildSettings()
        {
            var oldScenes = EditorBuildSettings.scenes;
            System.Collections.Generic.List<EditorBuildSettingsScene> newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            
            newScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/LoadingScene.unity", true));
            newScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true));
            newScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/TestScene.unity", true));

            foreach (var old in oldScenes)
            {
                if (!old.path.Contains("LoadingScene.unity") && 
                    !old.path.Contains("MainMenuScene.unity") && 
                    !old.path.Contains("TestScene.unity"))
                {
                    newScenes.Add(old);
                }
            }

            EditorBuildSettings.scenes = newScenes.ToArray();
        }

        // ======================= UI HELPERS =======================

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasObj = new GameObject(name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.AddComponent<SafeAreaController>();
            return canvasObj;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
        }

        private static GameObject CreateImage(GameObject parent, string name, string spriteName, Color col, bool stretch, Vector2 size = default)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            Image img = obj.AddComponent<Image>();
            img.color = col;
            
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/Sprites/{spriteName}.png");
            if (s != null)
            {
                img.sprite = s;
                if (s.border != Vector4.zero) img.type = Image.Type.Sliced;
            }

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.sizeDelta = size;
            }
            return obj;
        }

        private static GameObject CreateText(GameObject parent, string name, string text, int fontSize, Color col, Vector2 pos)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = col;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(1200, fontSize + 50);

            return obj;
        }

        private static void AddOutline(GameObject obj, Color c, int dist)
        {
            Outline o = obj.AddComponent<Outline>();
            o.effectColor = c;
            o.effectDistance = new Vector2(dist, -dist);
        }

        private static GameObject CreateButton(GameObject parent, string name, string spriteName, Vector2 pos, Vector2 size)
        {
            GameObject obj = CreateImage(parent, name, spriteName, Color.white, false, size);
            obj.GetComponent<RectTransform>().anchoredPosition = pos;
            obj.AddComponent<Button>();
            return obj;
        }

        private static GameObject CreateProgressBar(GameObject parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject frame = CreateImage(parent, name, "Progress_Frame", Color.white, false, size);
            frame.GetComponent<RectTransform>().anchoredPosition = pos;

            GameObject fill = CreateImage(frame, "Fill", "Progress_Fill", Color.white, false, size);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0);
            fillRt.anchorMax = new Vector2(1, 1);
            fillRt.offsetMin = new Vector2(8, 8);
            fillRt.offsetMax = new Vector2(-8, -8);
            
            Slider slider = frame.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fillRt;

            return frame;
        }

        private static GameObject CreateSlider(GameObject parent, string name, Vector2 pos, Vector2 size = default)
        {
            if (size == default) size = new Vector2(600, 32);
            GameObject frame = CreateImage(parent, name, "Slider_Track", Color.white, false, size);
            frame.GetComponent<RectTransform>().anchoredPosition = pos;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(frame.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0);
            fillAreaRt.anchorMax = new Vector2(1, 1);
            fillAreaRt.offsetMin = new Vector2(16, 0);
            fillAreaRt.offsetMax = new Vector2(-16, 0);

            GameObject fill = CreateImage(fillArea, "Fill", "Progress_Fill", Color.clear, true); // No visible fill needed for this slider style since track is visible
            
            GameObject handleArea = new GameObject("Handle Area");
            handleArea.transform.SetParent(frame.transform, false);
            RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = new Vector2(0, 0);
            handleAreaRt.anchorMax = new Vector2(1, 1);
            handleAreaRt.offsetMin = new Vector2(16, 0);
            handleAreaRt.offsetMax = new Vector2(-16, 0);

            GameObject handle = CreateImage(handleArea, "Handle", "Slider_Knob", Color.white, false, new Vector2(64, 64));

            Slider slider = frame.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.value = 1f;

            return frame;
        }
    }
}
