using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI; // For Image and Shadow

public class JiggleOnScreenButton : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Seconds between juggle animations")]
    private float juggleInterval = 30f; // Configurable in Editor

    [SerializeField]
    [Tooltip("Delay in seconds before the first juggle animation")]
    private float initialDelay = 2f; // Configurable in Editor, default 2 seconds

    [SerializeField]
    [Tooltip("The TextMeshPro hint text above the button")]
    private TextMeshProUGUI buttonHint;

    [SerializeField]
    [Tooltip("Duration of the slide animation in seconds")]
    private float slideDuration = 0.5f; // Slide in/out time

    [SerializeField]
    [Tooltip("Duration of the pulse and jiggle animation in seconds")]
    private float juggleDuration = 0.5f; // Pulse and jiggle time

    [SerializeField]
    [Tooltip("Scale multiplier for the pulse effect (e.g., 1.1 = 10% bigger)")]
    private float pulseScale = 1.1f; // How much to scale up

    [SerializeField]
    [Tooltip("Degrees to tilt left and right for the jiggle effect")]
    private float jiggleAngle = 5f; // Tilt angle (e.g., 5 degrees each way)

    [SerializeField]
    [Tooltip("Total distance to slide (in screen units), split as 1 left : 2 down")]
    private float slideDistance = 50f; // How far to slide

    private Image buttonImage; // Reference to the background Image
    private Vector3 originalButtonPosition; // Store original position of the button
    private Vector3 originalButtonScale; // Store original scale of the button
    private Quaternion originalButtonRotation; // Store original rotation of the button
    private Vector3 originalHintPosition; // Store original position of the hint
    private Vector3 originalHintScale; // Store original scale of the hint
    private Quaternion originalHintRotation; // Store original rotation of the hint
    private float lastJuggleTime = float.MinValue; // Track the last juggle, start far in past
    private bool isFirstJuggle = true; // Flag for the first delayed juggle

    void Awake()
    {
        // Get the background Image component
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError("JiggleOnScreenButton requires an Image component on the GameObject.");
        }

        // Validate the hint text
        if (buttonHint == null)
        {
            TextMeshProUGUI[] tmps = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var tmp in tmps)
            {
                if (tmp != GetComponentInChildren<TextMeshProUGUI>(true)) // Assume first TMP is the label
                {
                    buttonHint = tmp;
                    break;
                }
            }
            if (buttonHint == null)
            {
                Debug.LogError("JiggleOnScreenButton requires a TextMeshProUGUI hint text assigned.");
            }
        }

        // Store original positions, scales, and rotations
        originalButtonPosition = transform.position;
        originalButtonScale = transform.localScale;
        originalButtonRotation = transform.localRotation;
        if (buttonHint != null)
        {
            originalHintPosition = buttonHint.transform.position;
            originalHintScale = buttonHint.transform.localScale;
            originalHintRotation = buttonHint.transform.localRotation;
        }
    }

    void Update()
    {
        // Check if it’s time to juggle
        if (isFirstJuggle)
        {
            // First juggle: wait for initialDelay
            if (Time.time >= initialDelay)
            {
                lastJuggleTime = Time.time;
                StartJuggleAnimation();
                isFirstJuggle = false;
            }
        }
        else
        {
            // Subsequent jiggles: every juggleInterval
            if (Time.time - lastJuggleTime >= juggleInterval)
            {
                lastJuggleTime = Time.time;
                StartJuggleAnimation();
            }
        }
    }

    void StartJuggleAnimation()
    {
        // Cancel any existing tweens
        LeanTween.cancel(gameObject);
        if (buttonHint != null)
        {
            LeanTween.cancel(buttonHint.gameObject);
        }

        // Calculate slide vector: 1 unit left, 2 units down
        Vector2 slideVector = new Vector2(-slideDistance / 3f, -2 * slideDistance / 3f); // Total distance split 1:2

        // Slide in the button
        LeanTween.move(gameObject, originalButtonPosition + new Vector3(slideVector.x, slideVector.y, 0), slideDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Pulse and jiggle the button
                PulseAndJiggle(gameObject, originalButtonScale, originalButtonRotation);

                // Slide back the button
                LeanTween.move(gameObject, originalButtonPosition, slideDuration)
                    .setEase(LeanTweenType.easeInQuad)
                    .setDelay(juggleDuration); // Wait for pulse/jiggle to finish
            });

        // Slide in the hint text
        if (buttonHint != null)
        {
            LeanTween.move(buttonHint.gameObject, originalHintPosition + new Vector3(slideVector.x, slideVector.y, 0), slideDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    // Pulse and jiggle the hint
                    PulseAndJiggle(buttonHint.gameObject, originalHintScale, originalHintRotation);

                    // Slide back the hint
                    LeanTween.move(buttonHint.gameObject, originalHintPosition, slideDuration)
                        .setEase(LeanTweenType.easeInQuad)
                        .setDelay(juggleDuration); // Wait for pulse/jiggle to finish
                });
        }
    }

    void PulseAndJiggle(GameObject target, Vector3 originalScale, Quaternion originalRotation)
    {
        // Pulse (scale)
        LeanTween.scale(target, originalScale * pulseScale, juggleDuration / 2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(target, originalScale, juggleDuration / 2f)
                    .setEase(LeanTweenType.easeInQuad);
            });

        // Jiggle (tilt)
        LeanTween.rotateZ(target, jiggleAngle, juggleDuration / 4f)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong(2) // Tilt left, right, then back
            .setOnComplete(() =>
            {
                target.transform.localRotation = originalRotation; // Reset rotation
            });
    }
}