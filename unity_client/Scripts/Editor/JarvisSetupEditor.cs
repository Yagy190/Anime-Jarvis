using UnityEngine;
using UnityEditor;

public class JarvisSetupEditor : EditorWindow
{
    [MenuItem("Jarvis/Auto-Setup Scene")]
    public static void SetupScene()
    {
        // 1. Setup Camera for Transparency
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0, 0, 0, 0); // Transparent black
            Debug.Log("Main Camera configured for transparency.");
        }
        else
        {
            Debug.LogWarning("Could not find Main Camera. Please set it up manually.");
        }

        // 2. Setup Player Settings for Overlay Mode
        PlayerSettings.runInBackground = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultIsNativeResolution = true;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.visibleInBackground = true;
        Debug.Log("Player Settings configured for Desktop Mascot mode.");

        // 3. Create Jarvis Controller GameObject
        GameObject jarvisController = GameObject.Find("JarvisController");
        if (jarvisController == null)
        {
            jarvisController = new GameObject("JarvisController");
            
            // Add scripts
            JarvisClient client = jarvisController.AddComponent<JarvisClient>();
            AudioPlayer audioPlayer = jarvisController.AddComponent<AudioPlayer>();
            jarvisController.AddComponent<TransparentWindow>();

            // Link them up
            client.audioPlayer = audioPlayer;
            
            Debug.Log("JarvisController created with all necessary scripts attached.");
        }
        else
        {
            Debug.Log("JarvisController already exists.");
        }

        EditorUtility.DisplayDialog("Jarvis Setup Complete", "The scene and player settings have been automatically configured!\n\n1. Drag your VRM avatar into the scene.\n2. Add the 'AvatarAnimator' script to your avatar.\n3. Drag your avatar into the 'Avatar Animator' slot on the JarvisController.", "Awesome!");
    }
}
