using UnityEngine;

/// <summary>
/// Ends the game once the player walks into the exit corridor after unlocking the door:
/// the world goes black and an end screen with the escape time appears in front of them.
/// </summary>
public class EscapeEnding : MonoBehaviour
{
    [Tooltip("Size of the area, centred on this object, that ends the game when the player's head enters it.")]
    [SerializeField] private Vector3 zoneSize = new Vector3(3f, 3f, 1.5f);
    [Tooltip("Text shown on the end screen. It should be on its own layer so it is the only thing left visible.")]
    [SerializeField] private TextMesh endScreen;
    [SerializeField] private float textDistance = 1.5f;

    [Tooltip("Pause before a button press on the end screen restarts the game.")]
    [SerializeField] private float restartDelay = 1f;

    private bool ended;
    private float endedAt;

    private const OVRInput.Button RestartButtons = OVRInput.Button.One | OVRInput.Button.Three
        | OVRInput.Button.PrimaryIndexTrigger | OVRInput.Button.SecondaryIndexTrigger;

    private void Start()
    {
        if (endScreen != null)
            endScreen.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (ended)
        {
            if (Time.time - endedAt > restartDelay && OVRInput.GetDown(RestartButtons))
                RestartGame.Restart();
            return;
        }

        var game = EscapeRoomGame.Instance;
        if (game == null || game.Stage != EscapeStage.Escaped)
            return;

        Transform head = XRRigUtility.Head;
        if (head != null && new Bounds(transform.position, zoneSize).Contains(head.position))
            ShowEndScreen(head);
    }

    private void ShowEndScreen(Transform head)
    {
        ended = true;
        endedAt = Time.time;
        if (endScreen == null)
            return;

        // Render nothing but the end screen text on a black background.
        var cam = head.GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 1 << endScreen.gameObject.layer;
        }

        int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad);
        endScreen.text = $"YOU ESCAPED!\n\nThanks for playing\n\nTime: {seconds / 60:00}:{seconds % 60:00}"
            + "\n\n<size=40>Press A or X, or pull a trigger,\nto play again</size>";
        endScreen.transform.SetParent(head, false);
        endScreen.transform.localPosition = new Vector3(0f, 0f, textDistance);
        endScreen.transform.localRotation = Quaternion.identity;
        endScreen.gameObject.SetActive(true);

        var audioSource = head.gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(PotionSounds.CreateChime());
    }
}
