using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Restarts the escape room: hold the controller's menu button, or press R on the keyboard
/// when testing in the Editor. The end screen also calls <see cref="Restart"/>.
/// </summary>
public class RestartGame : MonoBehaviour
{
    [SerializeField] private float holdToRestart = 1.5f;

    private float heldFor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (FindAnyObjectByType<EscapeRoomGame>() != null && FindAnyObjectByType<RestartGame>() == null)
            new GameObject(nameof(RestartGame)).AddComponent<RestartGame>();
    }

    public static void Restart()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex >= 0 ? scene.buildIndex : 0);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Restart();
            return;
        }

        heldFor = OVRInput.Get(OVRInput.Button.Start) ? heldFor + Time.unscaledDeltaTime : 0f;
        if (heldFor >= holdToRestart)
            Restart();
    }
}
