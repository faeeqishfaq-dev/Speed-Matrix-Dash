using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using GeometryDash.Level;

public class PhaseTenAttachCameraClamp
{
    [MenuItem("Tools/Geometry Dash/Phase 10: Attach MilestoneCameraClamp")]
    public static void AttachClamp()
    {
        string scenePath = "Assets/Scenes/TestScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null) mainCam = camObj.GetComponent<Camera>();
        }

        if (mainCam != null)
        {
            MilestoneCameraClamp clamp = mainCam.GetComponent<MilestoneCameraClamp>();
            if (clamp == null)
            {
                clamp = mainCam.gameObject.AddComponent<MilestoneCameraClamp>();
                Debug.Log("Attached MilestoneCameraClamp to Main Camera.");
            }
            else
            {
                Debug.Log("MilestoneCameraClamp already attached.");
            }
            
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("TestScene saved successfully.");
        }
        else
        {
            Debug.LogError("Main Camera not found in TestScene!");
        }
    }
}
