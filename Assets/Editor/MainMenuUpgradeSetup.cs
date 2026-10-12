using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using GeometryDash.UI;

namespace GeometryDash.Editor
{
    [InitializeOnLoad]
    public static class MainMenuUpgradeSetup
    {
        static MainMenuUpgradeSetup()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // Only run if not already applied
            if (NeedsUpgrade())
            {
                ExecuteSetup();
            }
        }

        [MenuItem("Tools/Geometry Dash/Setup Main Menu Upgrades")]
        public static void ExecuteSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[MainMenuUpgradeSetup] Cannot run setup while in Play Mode.");
                return;
            }

            string scenePath = "Assets/Scenes/MainMenuScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MainMenuUpgradeSetup] Could not open scene: {scenePath}");
                return;
            }

            // 1. Locate MainMenuCanvas
            GameObject canvasObj = GameObject.Find("MainMenuCanvas");
            if (canvasObj == null)
            {
                Debug.LogError("[MainMenuUpgradeSetup] MainMenuCanvas not found in MainMenuScene!");
                return;
            }

            // 2. Configure Play Button Float Animation
            Transform playBtnTrans = canvasObj.transform.Find("Btn_Play");
            if (playBtnTrans == null)
            {
                Debug.LogError("[MainMenuUpgradeSetup] Btn_Play not found under MainMenuCanvas!");
                return;
            }

            GameObject playBtnObj = playBtnTrans.gameObject;
            MainMenuPlayButtonFloat playFloat = playBtnObj.GetComponent<MainMenuPlayButtonFloat>();
            if (playFloat == null)
            {
                playFloat = playBtnObj.AddComponent<MainMenuPlayButtonFloat>();
            }
            playFloat.FloatDistance = 10f;
            playFloat.FloatSpeed = 1.6f;
            playFloat.EnableFloat = true;
            EditorUtility.SetDirty(playBtnObj);
            Debug.Log("[MainMenuUpgradeSetup] Successfully attached MainMenuPlayButtonFloat to Btn_Play.");

            // 3. Configure Settings Panel & Quit Button
            Transform settingsPanelTrans = canvasObj.transform.Find("SettingsPanel");
            if (settingsPanelTrans == null)
            {
                Debug.LogError("[MainMenuUpgradeSetup] SettingsPanel not found under MainMenuCanvas!");
                return;
            }

            GameObject settingsPanelObj = settingsPanelTrans.gameObject;
            ApplicationQuitController quitController = settingsPanelObj.GetComponent<ApplicationQuitController>();
            if (quitController == null)
            {
                quitController = settingsPanelObj.AddComponent<ApplicationQuitController>();
            }
            EditorUtility.SetDirty(settingsPanelObj);

            // Check if Btn_Quit already exists
            Transform quitBtnTrans = settingsPanelTrans.Find("Btn_Quit");
            GameObject btnQuitObj;
            if (quitBtnTrans != null)
            {
                btnQuitObj = quitBtnTrans.gameObject;
            }
            else
            {
                btnQuitObj = new GameObject("Btn_Quit");
                btnQuitObj.transform.SetParent(settingsPanelTrans, false);
            }

            // Configure RectTransform for Btn_Quit
            RectTransform quitRT = btnQuitObj.GetComponent<RectTransform>();
            if (quitRT == null) quitRT = btnQuitObj.AddComponent<RectTransform>();
            quitRT.anchorMin = new Vector2(0.5f, 0.5f);
            quitRT.anchorMax = new Vector2(0.5f, 0.5f);
            quitRT.pivot = new Vector2(0.5f, 0.5f);
            quitRT.anchoredPosition = new Vector2(0f, -325f);
            quitRT.sizeDelta = new Vector2(260f, 70f);

            // Configure Image for Btn_Quit
            Image btnImg = btnQuitObj.GetComponent<Image>();
            if (btnImg == null) btnImg = btnQuitObj.AddComponent<Image>();
            Sprite wideBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/Button_Wide_Green.png");
            if (wideBtnSprite != null)
            {
                btnImg.sprite = wideBtnSprite;
                btnImg.type = Image.Type.Sliced;
            }
            btnImg.color = Color.white;

            // Configure Button component
            Button quitBtn = btnQuitObj.GetComponent<Button>();
            if (quitBtn == null) quitBtn = btnQuitObj.AddComponent<Button>();
            quitBtn.targetGraphic = btnImg;

            // Wire OnClick to ApplicationQuitController.QuitGame persistently
            // Clear existing duplicate calls if any
            int callCount = quitBtn.onClick.GetPersistentEventCount();
            bool alreadyWired = false;
            for (int i = 0; i < callCount; i++)
            {
                if (quitBtn.onClick.GetPersistentTarget(i) == quitController &&
                    quitBtn.onClick.GetPersistentMethodName(i) == nameof(ApplicationQuitController.QuitGame))
                {
                    alreadyWired = true;
                    break;
                }
            }

            if (!alreadyWired)
            {
                for (int i = callCount - 1; i >= 0; i--)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(quitBtn.onClick, i);
                }
                UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, quitController.QuitGame);
            }

            // Configure Text child for Btn_Quit
            Transform textTrans = btnQuitObj.transform.Find("Text");
            GameObject textObj;
            if (textTrans != null)
            {
                textObj = textTrans.gameObject;
            }
            else
            {
                textObj = new GameObject("Text");
                textObj.transform.SetParent(btnQuitObj.transform, false);
            }

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            if (textRT == null) textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.anchoredPosition = Vector2.zero;

            Text txt = textObj.GetComponent<Text>();
            if (txt == null) txt = textObj.AddComponent<Text>();
            txt.text = "QUIT";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 38;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            EditorUtility.SetDirty(btnQuitObj);
            EditorUtility.SetDirty(textObj);

            // Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MainMenuUpgradeSetup] Successfully saved MainMenuScene with Play Button Float and Settings Quit Button!");
        }

        private static bool NeedsUpgrade()
        {
            string scenePath = "Assets/Scenes/MainMenuScene.unity";
            if (!System.IO.File.Exists(scenePath)) return false;

            string sceneContent = System.IO.File.ReadAllText(scenePath);
            return !sceneContent.Contains("MainMenuPlayButtonFloat") || !sceneContent.Contains("Btn_Quit");
        }
    }
}
