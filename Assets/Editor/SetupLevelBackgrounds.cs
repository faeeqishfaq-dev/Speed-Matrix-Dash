using UnityEngine;
using UnityEditor;
using System.IO;
using GeometryDash.Camera;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class SetupLevelBackgrounds
    {
        [MenuItem("Tools/Geometry Dash/Setup Level Backgrounds")]
        public static void ExecuteSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Sprite blueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/blue.jpeg");
            Sprite nightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/night.jpeg");
            Sprite fireSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/fire.jpeg");

            if (blueSprite == null || nightSprite == null || fireSprite == null)
            {
                Debug.LogWarning("[SetupLevelBackgrounds] Warning: One or more background sprites not found in Assets/Art/");
            }

            SetupCameraPrefab(blueSprite, nightSprite, fireSprite);
            SetupLevelPrefabs(blueSprite, nightSprite, fireSprite);
            SetupOpenSceneCamera(blueSprite, nightSprite, fireSprite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SetupLevelBackgrounds] Successfully configured responsive backgrounds for all 15 levels!");
        }

        private static void SetupCameraPrefab(Sprite blue, Sprite night, Sprite fire)
        {
            string prefabPath = "Assets/Prefabs/Levels/Main Camera.prefab";
            if (!File.Exists(prefabPath)) return;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return;

            try
            {
                ConfigureCameraBackground(root.transform, blue, night, fire);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetupOpenSceneCamera(Sprite blue, Sprite night, Sprite fire)
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                ConfigureCameraBackground(mainCam.transform, blue, night, fire);
                EditorUtility.SetDirty(mainCam.gameObject);
                if (!EditorApplication.isPlaying && mainCam.gameObject.scene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCam.gameObject.scene);
                }
            }
        }

        private static void ConfigureCameraBackground(Transform cameraTransform, Sprite blue, Sprite night, Sprite fire)
        {
            UnityEngine.Camera cam = cameraTransform.GetComponent<UnityEngine.Camera>();

            // 1. LevelBackground container
            Transform bgRoot = cameraTransform.Find("LevelBackground");
            if (bgRoot == null)
            {
                GameObject bgObj = new GameObject("LevelBackground");
                bgObj.transform.SetParent(cameraTransform, false);
                bgObj.transform.localPosition = new Vector3(0f, 0f, 15f);
                bgObj.transform.localRotation = Quaternion.identity;
                bgObj.transform.localScale = Vector3.one;
                bgRoot = bgObj.transform;
            }
            else
            {
                bgRoot.localPosition = new Vector3(0f, 0f, 15f);
                bgRoot.localRotation = Quaternion.identity;
                bgRoot.localScale = Vector3.one;
            }

            LevelBackgroundController controller = bgRoot.GetComponent<LevelBackgroundController>();
            if (controller == null) controller = bgRoot.gameObject.AddComponent<LevelBackgroundController>();

            // 2. BackgroundSprite renderer child
            Transform spriteChild = bgRoot.Find("BackgroundSprite");
            SpriteRenderer sr = null;
            if (spriteChild == null)
            {
                GameObject sprObj = new GameObject("BackgroundSprite");
                sprObj.transform.SetParent(bgRoot, false);
                sprObj.transform.localPosition = Vector3.zero;
                sprObj.transform.localRotation = Quaternion.identity;
                sprObj.transform.localScale = Vector3.one;
                spriteChild = sprObj.transform;
                sr = sprObj.AddComponent<SpriteRenderer>();
            }
            else
            {
                sr = spriteChild.GetComponent<SpriteRenderer>();
                if (sr == null) sr = spriteChild.gameObject.AddComponent<SpriteRenderer>();
                spriteChild.localPosition = Vector3.zero;
                spriteChild.localRotation = Quaternion.identity;
            }

            sr.sortingLayerName = "Default";
            sr.sortingOrder = -100;
            if (sr.sprite == null && fire != null) sr.sprite = fire;

            controller.targetCamera = cam;
            controller.backgroundRenderer = sr;
            controller.fitMode = LevelBackgroundController.FitMode.Cover;
            controller.localZDistance = 15f;
            controller.edgeBuffer = 1.005f;
            controller.blueBackground = blue;
            controller.nightBackground = night;
            controller.fireBackground = fire;

            controller.RecalculateBackgroundFit(true);
        }

        private static void SetupLevelPrefabs(Sprite blue, Sprite night, Sprite fire)
        {
            for (int i = 1; i <= 15; i++)
            {
                string path = $"Assets/Prefabs/Levels/Level_{i:D2}.prefab";
                if (!File.Exists(path)) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                try
                {
                    LevelDefinition def = root.GetComponent<LevelDefinition>();
                    if (def != null)
                    {
                        def.BackgroundSprite = GetSpriteForLevel(i, blue, night, fire);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        public static Sprite GetSpriteForLevel(int levelIndex, Sprite blue, Sprite night, Sprite fire)
        {
            switch (levelIndex)
            {
                case 1:  return fire;
                case 2:  return blue;
                case 3:  return fire;
                case 4:  return night;
                case 5:  return fire;
                case 6:  return blue;
                case 7:  return fire;
                case 8:  return night;
                case 9:  return fire;
                case 10: return blue;
                case 11: return fire;
                case 12: return night;
                case 13: return fire;
                case 14: return blue;
                case 15: return fire;
                default:
                    if (levelIndex % 2 != 0) return fire;
                    if (levelIndex % 4 == 2) return blue;
                    return night;
            }
        }
    }
}
