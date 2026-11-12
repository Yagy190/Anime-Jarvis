using UnityEngine;
using UniVRM10;

public class AvatarAnimator : MonoBehaviour
{
    private Vrm10Instance vrmInstance;

    void Start()
    {
        // Get the Vrm10Instance attached to the VRM 1.0 avatar
        vrmInstance = GetComponent<Vrm10Instance>();
        if (vrmInstance == null)
        {
            Debug.LogError("Vrm10Instance not found on the avatar! Make sure this script is attached to the root of your avatar.");
        }
    }

    public void SetEmotion(string emotion)
    {
        if (vrmInstance == null || vrmInstance.Runtime == null) return;

        // Reset emotions first
        vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Happy, 0f);
        vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Sad, 0f);
        vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Angry, 0f);
        vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Relaxed, 0f);
        vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Surprised, 0f);

        // Apply new emotion
        switch (emotion.ToLower())
        {
            case "happy":
                vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Happy, 1.0f);
                break;
            case "sad":
                vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Sad, 1.0f);
                break;
            case "thinking":
                vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Surprised, 0.5f);
                break;
            default:
                // Neutral, do nothing
                break;
        }
    }
}
