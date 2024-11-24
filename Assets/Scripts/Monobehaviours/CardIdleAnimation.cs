using UnityEngine;

/// <summary>
/// Adds a gentle idle animation to the card, making it sway to simulate swimming.
/// Can be enabled or disabled to prevent conflicts during interactions.
/// </summary>
public class CardIdleAnimation : MonoBehaviour
{
    [Header("Idle Animation Settings")]
    [Tooltip("Maximum degrees the card will sway left and right.")]
    public float swayAmount = 5f; // Degrees to sway

    [Tooltip("Speed of the sway animation.")]
    public float swaySpeed = 1f;   // Speed of the sway

    private Quaternion originalRotation;
    private float swayTimeOffset;
    private bool isIdleAnimationEnabled = true;

    void Start()
    {
        // Store the original local rotation of the card
        originalRotation = transform.localRotation;

        // Assign a random time offset to desynchronize the sway animations across multiple cards
        swayTimeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (isIdleAnimationEnabled)
        {
            // Calculate the sway angle using a sine wave for smooth oscillation
            float swayAngle = Mathf.Sin((Time.time * swaySpeed) + swayTimeOffset) * swayAmount;

            // Create a rotation quaternion based on the sway angle around the Z-axis
            Quaternion swayRotation = Quaternion.Euler(0f, 0f, swayAngle);

            // Apply the sway rotation on top of the original rotation
            transform.localRotation = originalRotation * swayRotation;
        }
    }

    /// <summary>
    /// Temporarily disables the idle animation to prevent conflicts during interactions.
    /// </summary>
    public void DisableIdleAnimation()
    {
        isIdleAnimationEnabled = false;

        // Optionally, reset the rotation to the current state to avoid visual snapping
        originalRotation = transform.localRotation;
    }

    /// <summary>
    /// Re-enables the idle animation after interactions are complete.
    /// </summary>
    public void EnableIdleAnimation()
    {
        isIdleAnimationEnabled = true;

        // Optionally, add a new sway time offset to desynchronize after re-enabling
        swayTimeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    /// <summary>
    /// Updates the original rotation if the card's rotation changes externally.
    /// Useful if other scripts modify the card's rotation.
    /// </summary>
    public void UpdateOriginalRotation()
    {
        originalRotation = transform.localRotation;
    }
}
