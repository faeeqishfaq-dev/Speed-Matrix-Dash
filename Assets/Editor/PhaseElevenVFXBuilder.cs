using UnityEngine;
using UnityEditor;
using GeometryDash.Level;

public class PhaseElevenVFXBuilder
{
    [MenuItem("Tools/Geometry Dash/Phase 11: Setup Milestone VFX")]
    public static void SetupVFX()
    {
        string prefabPath = "Assets/Prefabs/Environment/MilestoneFinishWall.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("MilestoneFinishWall prefab not found!");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        // Setup Textures
        Texture2D squareTex = new Texture2D(8, 8);
        for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) squareTex.SetPixel(x, y, Color.white);
        squareTex.Apply();
        
        Texture2D softTex = new Texture2D(64, 64);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = (x - 32f) / 32f;
                float dy = (y - 32f) / 32f;
                float dist = Mathf.Sqrt(dx*dx + dy*dy);
                float a = Mathf.Clamp01(1f - dist);
                softTex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        softTex.Apply();

        if (!System.IO.Directory.Exists("Assets/Sprites")) System.IO.Directory.CreateDirectory("Assets/Sprites");
        byte[] sqBytes = squareTex.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/Sprites/VFXSquare.png", sqBytes);
        byte[] softBytes = softTex.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/Sprites/VFXSoft.png", softBytes);
        AssetDatabase.Refresh();

        Texture2D sqAsset = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/VFXSquare.png");
        Texture2D softAsset = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/VFXSoft.png");
        Sprite sqSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFXSquare.png");
        Sprite softSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFXSoft.png");

        // Setup Materials
        Material softMat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        softMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        softMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        softMat.SetTexture("_MainTex", softAsset);
        AssetDatabase.CreateAsset(softMat, "Assets/Materials/AdditiveSoftMat.mat");

        Material sqMat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        sqMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        sqMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        sqMat.SetTexture("_MainTex", sqAsset);
        AssetDatabase.CreateAsset(sqMat, "Assets/Materials/AdditiveSquareMat.mat");

        // Setup Controller
        MilestoneWallVFXController vfx = instance.GetComponent<MilestoneWallVFXController>();
        if (vfx == null) vfx = instance.AddComponent<MilestoneWallVFXController>();

        // Destroy old VFX if any
        Transform oldGlow = instance.transform.Find("WallEnergyGlow");
        if (oldGlow != null) Object.DestroyImmediate(oldGlow.gameObject);
        Transform oldPart = instance.transform.Find("WallParticleEmitter");
        if (oldPart != null) Object.DestroyImmediate(oldPart.gameObject);

        // 1. Wall Energy Glow
        GameObject glowRoot = new GameObject("WallEnergyGlow");
        glowRoot.transform.SetParent(instance.transform);
        glowRoot.transform.localPosition = new Vector3(-0.5f, 5f, 0f); // inner/left face of a standard 1-unit width wall? Wait, wall scale is x=2. So left edge is at -1.

        GameObject outerGlow = new GameObject("OuterGlow");
        outerGlow.transform.SetParent(glowRoot.transform);
        outerGlow.transform.localPosition = Vector3.zero;
        SpriteRenderer outerSR = outerGlow.AddComponent<SpriteRenderer>();
        outerSR.sprite = softSprite;
        outerSR.material = softMat;
        outerSR.sortingOrder = 10;
        
        GameObject midGlow = new GameObject("MidGlow");
        midGlow.transform.SetParent(glowRoot.transform);
        midGlow.transform.localPosition = Vector3.zero;
        SpriteRenderer midSR = midGlow.AddComponent<SpriteRenderer>();
        midSR.sprite = softSprite;
        midSR.material = softMat;
        midSR.sortingOrder = 11;

        GameObject coreGlow = new GameObject("CoreGlow");
        coreGlow.transform.SetParent(glowRoot.transform);
        coreGlow.transform.localPosition = Vector3.zero;
        SpriteRenderer coreSR = coreGlow.AddComponent<SpriteRenderer>();
        coreSR.sprite = softSprite;
        coreSR.material = softMat;
        coreSR.sortingOrder = 12;

        vfx.outerGlow = outerSR;
        vfx.midGlow = midSR;
        vfx.coreGlow = coreSR;

        // 2. Wall Particle Emitter
        GameObject partRoot = new GameObject("WallParticleEmitter");
        partRoot.transform.SetParent(instance.transform);
        partRoot.transform.localPosition = new Vector3(-1f, 5f, 0f); // Left edge of wall

        GameObject mainPartObj = new GameObject("MainParticles");
        mainPartObj.transform.SetParent(partRoot.transform);
        mainPartObj.transform.localPosition = Vector3.zero;
        mainPartObj.transform.localRotation = Quaternion.Euler(0, -90, 0); // Emit leftward (Z forward is now left)
        ParticleSystem mainPS = mainPartObj.AddComponent<ParticleSystem>();
        var mMain = mainPS.main;
        mMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
        mMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 10f);
        mMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        mMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        mMain.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var mShape = mainPS.shape;
        mShape.shapeType = ParticleSystemShapeType.Box;
        mShape.scale = new Vector3(0.1f, 20f, 1f); // Thin vertical rectangle

        var mEm = mainPS.emission;
        mEm.rateOverTime = 5f; // Pre-leak
        
        var mCol = mainPS.colorOverLifetime;
        mCol.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        mCol.color = grad;

        var mSize = mainPS.sizeOverLifetime;
        mSize.enabled = true;
        AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1f, 0.4f));
        mSize.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ParticleSystemRenderer mainPSR = mainPartObj.GetComponent<ParticleSystemRenderer>();
        mainPSR.material = sqMat;
        mainPSR.renderMode = ParticleSystemRenderMode.Billboard;
        
        GameObject microPartObj = new GameObject("MicroParticles");
        microPartObj.transform.SetParent(partRoot.transform);
        microPartObj.transform.localPosition = Vector3.zero;
        microPartObj.transform.localRotation = Quaternion.Euler(0, -90, 0);
        ParticleSystem microPS = microPartObj.AddComponent<ParticleSystem>();
        var miMain = microPS.main;
        miMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        miMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        miMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        miMain.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var miShape = microPS.shape;
        miShape.shapeType = ParticleSystemShapeType.Box;
        miShape.scale = new Vector3(0.1f, 20f, 1f);

        var miEm = microPS.emission;
        miEm.rateOverTime = 20f;
        
        var miCol = microPS.colorOverLifetime;
        miCol.enabled = true;
        miCol.color = grad;
        
        ParticleSystemRenderer miPSR = microPartObj.GetComponent<ParticleSystemRenderer>();
        miPSR.material = sqMat;

        vfx.mainParticles = mainPS;
        vfx.microParticles = microPS;

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);

        Debug.Log("MilestoneFinishWall VFX Successfully Configured!");
    }
}
