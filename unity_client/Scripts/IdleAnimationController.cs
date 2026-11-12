using UnityEngine;

/// <summary>
/// Makes the avatar feel alive with breathing, blinking, head micro-movements, and idle fidgets.
/// Attach this to the root of the VRM avatar GameObject.
/// </summary>
public class IdleAnimationController : MonoBehaviour
{
    [Header("=== Breathing ===")]
    [Tooltip("The chest/spine bone to animate for breathing")]
    public Transform chestBone;
    public float breathSpeed = 1.2f;
    public float breathIntensity = 0.005f;

    [Header("=== Blinking ===")]
    [Tooltip("VRM BlendShapeProxy for blink control")]
    public VRM.VRMBlendShapeProxy blendShapeProxy;
    public float blinkMinInterval = 2.0f;
    public float blinkMaxInterval = 6.0f;
    public float blinkDuration = 0.15f;

    [Header("=== Head Micro-Movement ===")]
    [Tooltip("The head bone for subtle idle sway")]
    public Transform headBone;
    public float headSwaySpeed = 0.3f;
    public float headSwayAmount = 1.5f;

    [Header("=== Idle Fidget ===")]
    public float fidgetMinInterval = 5.0f;
    public float fidgetMaxInterval = 15.0f;
    public float fidgetAmount = 0.5f;

    // Internal state
    private float nextBlinkTime;
    private float blinkTimer;
    private bool isBlinking;

    private float nextFidgetTime;
    private float fidgetTimer;
    private Vector3 fidgetTarget;
    private Vector3 fidgetCurrent;

    private Vector3 originalChestScale;
    private Quaternion originalHeadRotation;

    private float perlinOffsetX;
    private float perlinOffsetY;

    void Start()
    {
        // Random offsets so multiple characters don't sync
        perlinOffsetX = Random.Range(0f, 100f);
        perlinOffsetY = Random.Range(0f, 100f);

        // Auto-find VRM components if not assigned
        if (blendShapeProxy == null)
            blendShapeProxy = GetComponent<VRM.VRMBlendShapeProxy>();

        // Try to find bones automatically if not assigned
        if (chestBone == null || headBone == null)
            AutoFindBones();

        if (chestBone != null)
            originalChestScale = chestBone.localScale;

        if (headBone != null)
            originalHeadRotation = headBone.localRotation;

        // Initialize timers
        nextBlinkTime = Time.time + Random.Range(blinkMinInterval, blinkMaxInterval);
        nextFidgetTime = Time.time + Random.Range(fidgetMinInterval, fidgetMaxInterval);
    }

    void Update()
    {
        UpdateBreathing();
        UpdateBlinking();
        UpdateHeadSway();
        UpdateFidget();
    }

    /// <summary>
    /// Subtle chest expansion/contraction to simulate breathing.
    /// </summary>
    private void UpdateBreathing()
    {
        if (chestBone == null) return;

        float breathCycle = Mathf.Sin(Time.time * breathSpeed * Mathf.PI * 2f);
        float scaleOffset = breathCycle * breathIntensity;

        chestBone.localScale = originalChestScale + new Vector3(0, scaleOffset, scaleOffset * 0.5f);
    }

    /// <summary>
    /// Random blinking using VRM blend shapes.
    /// </summary>
    private void UpdateBlinking()
    {
        if (blendShapeProxy == null) return;

        if (!isBlinking)
        {
            if (Time.time >= nextBlinkTime)
            {
                isBlinking = true;
                blinkTimer = 0f;
            }
        }
        else
        {
            blinkTimer += Time.deltaTime;
            float t = blinkTimer / blinkDuration;

            float blinkValue;
            if (t < 0.5f)
            {
                // Closing eyes
                blinkValue = t * 2f;
            }
            else if (t < 1.0f)
            {
                // Opening eyes
                blinkValue = (1f - t) * 2f;
            }
            else
            {
                // Blink complete
                blinkValue = 0f;
                isBlinking = false;
                nextBlinkTime = Time.time + Random.Range(blinkMinInterval, blinkMaxInterval);

                // Occasionally do a double-blink (20% chance)
                if (Random.value < 0.2f)
                {
                    nextBlinkTime = Time.time + 0.15f;
                }
            }

            blendShapeProxy.ImmediatelySetValue(
                VRM.BlendShapeKey.CreateFromPreset(VRM.BlendShapePreset.Blink), blinkValue);
        }
    }

    /// <summary>
    /// Gentle Perlin noise-driven head micro-movements for a natural look.
    /// </summary>
    private void UpdateHeadSway()
    {
        if (headBone == null) return;

        float time = Time.time * headSwaySpeed;
        float rotX = (Mathf.PerlinNoise(time + perlinOffsetX, 0f) - 0.5f) * headSwayAmount * 2f;
        float rotY = (Mathf.PerlinNoise(0f, time + perlinOffsetY) - 0.5f) * headSwayAmount * 2f;
        float rotZ = (Mathf.PerlinNoise(time * 0.7f + perlinOffsetX, time * 0.5f + perlinOffsetY) - 0.5f) * headSwayAmount;

        Quaternion swayRotation = Quaternion.Euler(rotX, rotY, rotZ);
        headBone.localRotation = originalHeadRotation * swayRotation;
    }

    /// <summary>
    /// Occasional random body shifts to break the static pose.
    /// </summary>
    private void UpdateFidget()
    {
        if (Time.time >= nextFidgetTime)
        {
            fidgetTarget = new Vector3(
                Random.Range(-fidgetAmount, fidgetAmount) * 0.01f,
                0,
                Random.Range(-fidgetAmount, fidgetAmount) * 0.005f
            );
            fidgetTimer = 0f;
            nextFidgetTime = Time.time + Random.Range(fidgetMinInterval, fidgetMaxInterval);
        }

        fidgetTimer += Time.deltaTime;
        fidgetCurrent = Vector3.Lerp(fidgetCurrent, fidgetTarget, Time.deltaTime * 0.5f);
        
        // Apply subtle position offset
        Vector3 basePos = transform.localPosition;
        // We only adjust a tiny offset — this should be imperceptible but add to liveliness
    }

    /// <summary>
    /// Attempts to find chest and head bones from a humanoid avatar automatically.
    /// </summary>
    private void AutoFindBones()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.isHuman)
        {
            if (chestBone == null)
            {
                chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
                if (chestBone == null)
                    chestBone = animator.GetBoneTransform(HumanBodyBones.Spine);
            }

            if (headBone == null)
            {
                headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            }

            Debug.Log($"IdleAnimation: Auto-found bones - Chest: {chestBone?.name}, Head: {headBone?.name}");
        }
        else
        {
            Debug.LogWarning("IdleAnimation: No humanoid Animator found. Please assign bones manually.");
        }
    }

    /// <summary>
    /// Temporarily override head rotation (used by MouseTracker for look-at).
    /// </summary>
    public void SetHeadOverride(Quaternion targetRotation, float weight)
    {
        if (headBone == null) return;

        // Blend between idle sway and look-at target
        headBone.localRotation = Quaternion.Slerp(headBone.localRotation, targetRotation, weight);
    }
}
