using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace GeometryDash.Editor
{
    public static class PhaseOneSetup
    {
        private const string PrefKey = "GeometryDashPhaseOneSetup_Completed";

        static PhaseOneSetup()
        {
            EditorApplication.update += RunSetupOnce;
        }

        private static void RunSetupOnce()
        {
            EditorApplication.update -= RunSetupOnce;

            if (EditorPrefs.GetBool(PrefKey, false))
                return;

            Debug.Log("Starting Automated Geometry Dash Phase 1 Setup...");

            CreateDirectories();
            CreateTagsAndLayers();
            CreateTextures();
            CreatePrefabs();
            CreateTestScene();

            EditorPrefs.SetBool(PrefKey, true);
            Debug.Log("Automated Setup Complete! Open 'Assets/Scenes/TestScene.unity' to play.");
        }

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Textures")) AssetDatabase.CreateFolder("Assets", "Textures");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        private static void CreateTagsAndLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            // Add Tag
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            bool foundTag = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Hazard") { foundTag = true; break; }
            }
            if (!foundTag)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                tagsProp.GetArrayElementAtIndex(0).stringValue = "Hazard";
            }

            // Add Layer
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            bool foundLayer = false;
            int emptyLayerIndex = -1;
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp.stringValue == "Ground") { foundLayer = true; break; }
                if (emptyLayerIndex == -1 && string.IsNullOrEmpty(sp.stringValue)) emptyLayerIndex = i;
            }

            if (!foundLayer && emptyLayerIndex != -1)
            {
                layersProp.GetArrayElementAtIndex(emptyLayerIndex).stringValue = "Ground";
            }

            tagManager.ApplyModifiedProperties();
        }

        private static void CreateTextures()
        {
            string path = "Assets/Textures/Square.png";
            if (!File.Exists(path))
            {
                Texture2D tex = new Texture2D(32, 32);
                Color[] colors = new Color[32 * 32];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.Refresh();
                
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32; 
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
        }

        private static void CreatePrefabs()
        {
            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Square.png");

            // 1. Death Fragment
            GameObject fragObj = new GameObject("DeathFragment");
            SpriteRenderer fragSr = fragObj.AddComponent<SpriteRenderer>();
            fragSr.sprite = squareSprite;
            fragSr.color = Color.cyan;
            fragObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            fragObj.AddComponent<BoxCollider2D>();
            Rigidbody2D fragRb = fragObj.AddComponent<Rigidbody2D>();
            fragRb.gravityScale = 3f;
            fragObj.AddComponent<GeometryDash.Player.DeathFragment>();
            
            string fragPath = "Assets/Prefabs/DeathFragment.prefab";
            GameObject savedFrag = PrefabUtility.SaveAsPrefabAsset(fragObj, fragPath);
            GameObject.DestroyImmediate(fragObj);

            // 2. Death Particles
            GameObject ptObj = new GameObject("DeathParticles");
            ParticleSystem ps = ptObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 10f;
            main.startSize = 0.2f;
            main.loop = false;
            main.playOnAwake = false;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 20) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            
            string ptPath = "Assets/Prefabs/DeathParticles.prefab";
            GameObject savedPt = PrefabUtility.SaveAsPrefabAsset(ptObj, ptPath);
            GameObject.DestroyImmediate(ptObj);

            // 3. Player Prefab
            GameObject playerObj = new GameObject("CubePlayer");
            
            // Visual child
            GameObject visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(playerObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            SpriteRenderer pSr = visualObj.AddComponent<SpriteRenderer>();
            pSr.sprite = squareSprite;
            pSr.color = Color.cyan;
            visualObj.AddComponent<GeometryDash.Player.CubeVisualController>();

            // Ground Sensor
            GameObject sensorObj = new GameObject("GroundSensor");
            sensorObj.transform.SetParent(playerObj.transform);
            sensorObj.transform.localPosition = Vector3.zero;
            GeometryDash.Player.PlayerGroundSensor sensor = sensorObj.AddComponent<GeometryDash.Player.PlayerGroundSensor>();
            SerializedObject sensorSo = new SerializedObject(sensor);
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            if (groundLayerIndex == -1) groundLayerIndex = 8; // fallback to user layer 8
            sensorSo.FindProperty("groundLayer").intValue = 1 << groundLayerIndex;
            sensorSo.ApplyModifiedProperties();

            // Root components
            BoxCollider2D bc = playerObj.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(0.95f, 0.95f);
            Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 5f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            playerObj.AddComponent<GeometryDash.Player.PlayerInputController>();
            playerObj.AddComponent<GeometryDash.Player.CubePlayerController>();
            
            GeometryDash.Player.PlayerDeathController deathCtrl = playerObj.AddComponent<GeometryDash.Player.PlayerDeathController>();
            SerializedObject deathSo = new SerializedObject(deathCtrl);
            deathSo.FindProperty("deathFragmentPrefab").objectReferenceValue = savedFrag;
            deathSo.FindProperty("visualCube").objectReferenceValue = visualObj;
            
            GameObject deathPtInstance = (GameObject)PrefabUtility.InstantiatePrefab(savedPt);
            deathPtInstance.transform.SetParent(playerObj.transform);
            deathPtInstance.transform.localPosition = Vector3.zero;
            deathSo.FindProperty("deathParticles").objectReferenceValue = deathPtInstance.GetComponent<ParticleSystem>();
            deathSo.ApplyModifiedProperties();

            // Trail
            GameObject trailObj = new GameObject("Trail");
            trailObj.transform.SetParent(playerObj.transform);
            trailObj.transform.localPosition = new Vector3(-0.4f, -0.4f, 0f);
            TrailRenderer tr = trailObj.AddComponent<TrailRenderer>();
            tr.time = 0.3f;
            tr.startWidth = 0.8f;
            tr.endWidth = 0.0f;
            tr.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
            
            GeometryDash.Player.PlayerVFXController vfxCtrl = playerObj.AddComponent<GeometryDash.Player.PlayerVFXController>();
            SerializedObject vfxSo = new SerializedObject(vfxCtrl);
            vfxSo.FindProperty("trailRenderer").objectReferenceValue = tr;
            vfxSo.ApplyModifiedProperties();

            string playerPath = "Assets/Prefabs/CubePlayer.prefab";
            PrefabUtility.SaveAsPrefabAsset(playerObj, playerPath);
            GameObject.DestroyImmediate(playerObj);
        }

        private static void CreateTestScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Camera
            GameObject camObj = new GameObject("Main Camera");
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            camObj.tag = "MainCamera";
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.25f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            GeometryDash.Camera.SimpleCameraController camCtrl = camObj.AddComponent<GeometryDash.Camera.SimpleCameraController>();

            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Square.png");

            // Floor
            GameObject floor = new GameObject("Floor");
            floor.layer = LayerMask.NameToLayer("Ground");
            SpriteRenderer floorSr = floor.AddComponent<SpriteRenderer>();
            floorSr.sprite = squareSprite;
            floorSr.color = new Color(0.1f, 0.3f, 0.8f);
            floor.transform.localScale = new Vector3(200f, 4f, 1f);
            floor.transform.position = new Vector3(80f, -2f, 0f);
            floor.AddComponent<BoxCollider2D>();

            // Spikes
            for (int i = 0; i < 5; i++)
            {
                GameObject spike = new GameObject("Spike");
                spike.tag = "Hazard";
                SpriteRenderer spikeSr = spike.AddComponent<SpriteRenderer>();
                spikeSr.sprite = squareSprite;
                spikeSr.color = Color.red;
                spike.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                spike.transform.position = new Vector3(15f + (i * 20f), 0.3f, 0f);
                spike.transform.rotation = Quaternion.Euler(0, 0, 45);
                BoxCollider2D spikeBc = spike.AddComponent<BoxCollider2D>();
                spikeBc.isTrigger = true;
            }

            // Platform
            GameObject plat = new GameObject("Platform");
            plat.layer = LayerMask.NameToLayer("Ground");
            SpriteRenderer platSr = plat.AddComponent<SpriteRenderer>();
            platSr.sprite = squareSprite;
            platSr.color = new Color(0.1f, 0.3f, 0.8f);
            plat.transform.localScale = new Vector3(6f, 2f, 1f);
            plat.transform.position = new Vector3(30f, 1f, 0f);
            plat.AddComponent<BoxCollider2D>();

            // Player Instance
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CubePlayer.prefab");
            GameObject playerInst = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerInst.transform.position = new Vector3(0, 0.5f, 0);

            camCtrl.SetTarget(playerInst.transform);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/TestScene.unity");
        }
    }
}
