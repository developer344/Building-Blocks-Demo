using System.Collections;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// The exit door. Touching the key anywhere on the door unlocks it and swings it open on its hinge.
/// Place this on the hinge object; the door model is its child.
/// </summary>
public class ExitDoor : MonoBehaviour
{
    [SerializeField] private Grabbable key;
    [Tooltip("Where the unlock sparkles appear.")]
    [SerializeField] private Transform lockPoint;
    [Tooltip("How close any part of the key has to come to the door to unlock it.")]
    [SerializeField] private float unlockReach = 0.3f;
    [SerializeField] private float openAngle = -100f;
    [SerializeField] private float openDuration = 2f;
    [SerializeField] private ParticleSystem unlockSparkles;
    [SerializeField] private GameObject escapedMessage;

    private bool unlocked;
    private Collider doorCollider;

    private void Start()
    {
        doorCollider = GetComponentInChildren<Collider>();
        if (escapedMessage != null)
            escapedMessage.SetActive(false);
    }

    private void Update()
    {
        var game = EscapeRoomGame.Instance;
        if (unlocked || game == null || key == null)
            return;

        if (game.Stage == EscapeStage.FindKey && key.SelectingPointsCount > 0)
            game.SetStage(EscapeStage.UnlockDoor);

        bool keyAvailable = game.Stage == EscapeStage.FindKey || game.Stage == EscapeStage.UnlockDoor;
        if (keyAvailable && key.gameObject.activeInHierarchy && IsKeyAtDoor())
            Unlock(game);
    }

    private bool IsKeyAtDoor()
    {
        var keyCollider = key.GetComponent<Collider>();
        Bounds keyBounds = keyCollider != null ? keyCollider.bounds : XRRigUtility.RendererBounds(key.transform);

        if (doorCollider == null)
            return lockPoint != null && keyBounds.SqrDistance(lockPoint.position) < unlockReach * unlockReach;

        Vector3 nearestOnDoor = doorCollider.ClosestPoint(keyBounds.center);
        return keyBounds.SqrDistance(nearestOnDoor) < unlockReach * unlockReach;
    }

    private void Unlock(EscapeRoomGame game)
    {
        unlocked = true;
        key.gameObject.SetActive(false);
        if (unlockSparkles != null)
            unlockSparkles.Play();
        game.SetStage(EscapeStage.Escaped);
        StartCoroutine(SwingOpen());
    }

    private IEnumerator SwingOpen()
    {
        Quaternion closed = transform.localRotation;
        Quaternion open = closed * Quaternion.Euler(0f, openAngle, 0f);
        for (float t = 0f; t < openDuration; t += Time.deltaTime)
        {
            transform.localRotation = Quaternion.Slerp(closed, open, Mathf.SmoothStep(0f, 1f, t / openDuration));
            yield return null;
        }
        transform.localRotation = open;

        if (escapedMessage != null)
            escapedMessage.SetActive(true);
    }
}
