using UnityEngine;
using UnityEditor;
using System.IO;
using GeometryDash.Level;

namespace GeometryDash.Editor
{
    public static class MakeWallVFXPermanent
    {
        [MenuItem("Tools/Geometry Dash/Make Wall VFX Permanent")]
        public static void ExecuteSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            SetupMilestoneFinishWallPrefab();
            SetupLevelPrefab("Assets/Prefabs/Levels/Level_04.prefab");
            SetupLevelPrefab("Assets/Prefabs/Levels/Level_08.prefab");
            SetupLevelPrefab("Assets/Prefabs/Levels/Level_12.prefab");
            SetupOpenSceneObjects();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MakeWallVFXPermanent] Successfully made WallEnergyGlow and WallParticleEmitter permanent editor objects!");
        }

        private static void SetupMilestoneFinishWallPrefab()
        {
            string prefabPath = "Assets/Prefabs/Environment/MilestoneFinishWall.prefab";
            if (!File.Exists(prefabPath)) return;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return;

            try
            {
                ApplyVFXHierarchy(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetupLevelPrefab(string prefabPath)
        {
            if (!File.Exists(prefabPath)) return;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return;

            try
            {
                Transform finish = root.transform.Find("MilestoneFinish");
                if (finish != null)
                {
                    ApplyVFXHierarchy(finish);
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetupOpenSceneObjects()
        {
            // Search active scene for any Level_4 or MilestoneFinish objects
            var finishes = Object.FindObjectsOfType<MilestoneFinishTrigger>(true);
            foreach (var trigger in finishes)
            {
                Transform finishRoot = trigger.transform.parent != null ? trigger.transform.parent : trigger.transform;
                ApplyVFXHierarchy(finishRoot);
                EditorUtility.SetDirty(finishRoot.gameObject);
            }

            GameObject level4 = GameObject.Find("Level_4");
            if (level4 != null)
            {
                Transform finish = level4.transform.Find("MilestoneFinish");
                if (finish != null)
                {
                    ApplyVFXHierarchy(finish);
                    EditorUtility.SetDirty(finish.gameObject);
                }
                EditorUtility.SetDirty(level4);
            }
        }

        public static void ApplyVFXHierarchy(Transform wallRoot)
        {
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneGlowBar.png");
            if (glowSprite == null) glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneCircle.png");

            // 1. Ensure MilestoneWallVFXController exists on root
            MilestoneWallVFXController vfxCtrl = wallRoot.GetComponent<MilestoneWallVFXController>();
            if (vfxCtrl == null) vfxCtrl = wallRoot.gameObject.AddComponent<MilestoneWallVFXController>();

            // 2. WallEnergyGlow
            Transform glowRoot = wallRoot.Find("WallEnergyGlow");
            if (glowRoot == null)
            {
                GameObject gObj = new GameObject("WallEnergyGlow");
                gObj.transform.SetParent(wallRoot, false);
                gObj.transform.localPosition = new Vector3(0f, 5f, 0f);
                gObj.transform.localRotation = Quaternion.identity;
                gObj.transform.localScale = Vector3.one;
                glowRoot = gObj.transform;
            }

            // OuterGlow
            SpriteRenderer outerSR = EnsureGlowChild(glowRoot, "OuterGlow", new Vector3(0f, 0f, 0f), new Vector3(5f, 20f, 1f), new Color(1f, 0.6f, 0.13f, 0.20f), 10, glowSprite);
            // PurpleWallGlow
            SpriteRenderer purpleSR = EnsureGlowChild(glowRoot, "PurpleWallGlow", new Vector3(1f, 0f, 0f), new Vector3(3.5f, 20f, 1f), new Color(0.75f, 0.15f, 1f, 0.30f), 11, glowSprite);
            // MidGlow
            SpriteRenderer midSR = EnsureGlowChild(glowRoot, "MidGlow", new Vector3(0f, 0f, 0f), new Vector3(2f, 20f, 1f), new Color(1f, 0.84f, 0.16f, 0.35f), 12, glowSprite);
            // CoreGlow
            SpriteRenderer coreSR = EnsureGlowChild(glowRoot, "CoreGlow", new Vector3(0f, 0f, 0f), new Vector3(0.4f, 20f, 1f), new Color(1f, 0.98f, 0.82f, 0.65f), 13, glowSprite);

            // 3. WallParticleEmitter
            Transform partRoot = wallRoot.Find("WallParticleEmitter");
            if (partRoot == null)
            {
                GameObject pObj = new GameObject("WallParticleEmitter");
                pObj.transform.SetParent(wallRoot, false);
                pObj.transform.localPosition = new Vector3(0f, 5f, 0f);
                pObj.transform.localRotation = Quaternion.identity;
                pObj.transform.localScale = Vector3.one;
                partRoot = pObj.transform;
            }

            // MainParticles
            Transform mainPartTransform = partRoot.Find("MainParticles");
            ParticleSystem mainPS = null;
            if (mainPartTransform == null)
            {
                GameObject mpObj = new GameObject("MainParticles");
                mpObj.transform.SetParent(partRoot, false);
                mpObj.transform.localPosition = Vector3.zero;
                mpObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                mpObj.transform.localScale = Vector3.one;

                mainPS = mpObj.AddComponent<ParticleSystem>();
                var main = mainPS.main;
                main.duration = 5f;
                main.loop = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 2.2f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.playOnAwake = true;
                main.maxParticles = 1000;

                var emission = mainPS.emission;
                emission.enabled = true;
                emission.rateOverTime = 12f;

                var shape = mainPS.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(0.1f, 18f, 1f);

                var colorOverLifetime = mainPS.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient g = new Gradient();
                g.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(new Color(1f, 0.95f, 0.4f), 0f),
                        new GradientColorKey(new Color(1f, 0.6f, 0.1f), 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(0.8f, 0.6f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                colorOverLifetime.color = g;

                var sizeOverLifetime = mainPS.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

                ParticleSystemRenderer psRenderer = mpObj.GetComponent<ParticleSystemRenderer>();
                psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                psRenderer.sortingOrder = 15;
                // Leave material unassigned as specifically requested so the user can configure it manually in Inspector!
            }
            else
            {
                mainPS = mainPartTransform.GetComponent<ParticleSystem>();
            }

            // 4. LightBeamEmitter
            Transform beamRoot = wallRoot.Find("LightBeamEmitter");
            if (beamRoot == null)
            {
                GameObject bObj = new GameObject("LightBeamEmitter");
                bObj.transform.SetParent(wallRoot, false);
                bObj.transform.localPosition = new Vector3(1f, 5f, 0f);
                bObj.transform.localRotation = Quaternion.identity;
                bObj.transform.localScale = Vector3.one;
                beamRoot = bObj.transform;
            }
            else
            {
                beamRoot.localPosition = new Vector3(1f, 5f, 0f);
                beamRoot.localRotation = Quaternion.identity;
                beamRoot.localScale = Vector3.one;
            }

            Sprite raySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MilestoneRay.png");
            float[] angles = { 45f, 65f, 82f, 98f, 115f, 135f };
            float[] widths = { 0.40f, 0.48f, 0.52f, 0.50f, 0.45f, 0.38f };
            Color[] colors = {
                new Color(1.0f, 0.90f, 0.40f, 0.85f),
                new Color(1.0f, 0.98f, 0.75f, 0.95f),
                new Color(1.0f, 0.85f, 0.25f, 0.90f),
                new Color(1.0f, 0.92f, 0.60f, 0.90f),
                new Color(1.0f, 0.78f, 0.20f, 0.85f),
                new Color(1.0f, 0.88f, 0.35f, 0.80f)
            };

            SpriteRenderer[] beams = new SpriteRenderer[6];
            for (int i = 0; i < 6; i++)
            {
                string beamName = $"Beam_0{i + 1}";
                Transform beamChild = beamRoot.Find(beamName);
                SpriteRenderer sr = null;
                if (beamChild == null)
                {
                    GameObject bChildObj = new GameObject(beamName);
                    bChildObj.transform.SetParent(beamRoot, false);
                    beamChild = bChildObj.transform;
                    sr = bChildObj.AddComponent<SpriteRenderer>();
                }
                else
                {
                    sr = beamChild.GetComponent<SpriteRenderer>();
                    if (sr == null) sr = beamChild.gameObject.AddComponent<SpriteRenderer>();
                }

                beamChild.localPosition = Vector3.zero;
                beamChild.localRotation = Quaternion.Euler(0f, 0f, angles[i]);
                beamChild.localScale = new Vector3(widths[i], 0f, 1f);

                if (raySprite != null) sr.sprite = raySprite;
                sr.color = colors[i];
                sr.sortingOrder = 25;
                beams[i] = sr;
            }

            // Assign references on MilestoneWallVFXController
            vfxCtrl.outerGlow = outerSR;
            vfxCtrl.purpleWallGlow = purpleSR;
            vfxCtrl.midGlow = midSR;
            vfxCtrl.coreGlow = coreSR;
            vfxCtrl.mainParticles = mainPS;
            vfxCtrl.lightBeamEmitter = beamRoot.gameObject;
            vfxCtrl.beamRenderers = beams;
            vfxCtrl.beamGroup2Delay = 0.12f;
            vfxCtrl.beamGroup3Delay = 0.24f;
            vfxCtrl.enableBeamShake = true;
            vfxCtrl.beamShakeStrength = 0.5f;
            vfxCtrl.beamShakeFrequency = 20f;
        }

        private static SpriteRenderer EnsureGlowChild(Transform parent, string childName, Vector3 localPos, Vector3 localScale, Color color, int sortingOrder, Sprite sprite)
        {
            Transform child = parent.Find(childName);
            SpriteRenderer sr = null;
            if (child == null)
            {
                GameObject obj = new GameObject(childName);
                obj.transform.SetParent(parent, false);
                obj.transform.localPosition = localPos;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = localScale;
                sr = obj.AddComponent<SpriteRenderer>();
            }
            else
            {
                sr = child.GetComponent<SpriteRenderer>();
                if (sr == null) sr = child.gameObject.AddComponent<SpriteRenderer>();
                child.localPosition = localPos;
                child.localScale = localScale;
            }

            if (sr.sprite == null && sprite != null) sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return sr;
        }
    }
}
