using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.SceneManagement;
using LocomotionCharacter = Oculus.Interaction.Locomotion.CharacterController;

/// <summary>
/// Stops the player walking through walls with real (room-scale) head movement.
/// The Interaction SDK keeps the body capsule out of walls but lets the head drift into them;
/// this puts the player back on the capsule as soon as the head gets ahead of it.
/// </summary>
public class WallPushback : MonoBehaviour
{
    [Tooltip("How far the head may get ahead of the body capsule before being pushed back.")]
    [SerializeField] private float maxHeadOffset = 0.15f;
    [Tooltip("Larger gaps come from teleporting, which the locomotor resolves on its own.")]
    [SerializeField] private float teleportGap = 1f;

    private const float HeadCenterBehindEyes = 0.0965f;

    private FirstPersonLocomotor locomotor;
    private LocomotionCharacter character;

    // Hooked to every scene load so it comes back after the game is restarted.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (FindAnyObjectByType<WallPushback>() == null && FindAnyObjectByType<FirstPersonLocomotor>() != null)
            new GameObject(nameof(WallPushback)).AddComponent<WallPushback>();
    }

    private void LateUpdate()
    {
        if (locomotor == null || character == null)
        {
            locomotor = FindAnyObjectByType<FirstPersonLocomotor>();
            character = FindAnyObjectByType<LocomotionCharacter>();
            if (locomotor == null || character == null)
                return;
        }

        Transform head = XRRigUtility.Head;
        if (head == null)
            return;

        // Same head point the locomotor tracks: the middle of the head, a little behind the eyes.
        Vector3 headCenter = head.position - head.forward * HeadCenterBehindEyes;
        Vector3 gap = headCenter - character.Pose.position;
        gap.y = 0f;
        float distance = gap.magnitude;
        if (distance > maxHeadOffset && distance < teleportGap)
            locomotor.ResetPlayerToCharacter();
    }
}
