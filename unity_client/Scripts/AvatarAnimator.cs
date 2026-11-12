using UnityEngine;

// Requires VRM/UniVRM package to be imported for VRMBlendShapeProxy
using VRM; 

public class AvatarAnimator : MonoBehaviour
{
    private VRMBlendShapeProxy blendShapeProxy;

    void Start()
    {
        // Get the VRMBlendShapeProxy attached to the avatar
        blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            Debug.LogError("VRMBlendShapeProxy not found on the avatar!");
        }
    }

    public void SetEmotion(string emotion)
    {
        if (blendShapeProxy == null) return;

        // Reset all emotions first
        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0);
        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0);
        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 0);
        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 0);

        // Apply new emotion
        switch (emotion.ToLower())
        {
            case "happy":
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 1.0f);
                break;
            case "sad":
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 1.0f);
                break;
            case "thinking":
                // For 'thinking' we might just use a slight 'Fun' or neutral look, or an 'O' shape slightly.
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 0.3f);
                break;
            default:
                // Neutral, do nothing
                break;
        }
    }
}
