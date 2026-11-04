using UnityEngine;

/// <summary>
/// Holds the potion after it is filled at the cauldron. Holding it to the player's mouth for a
/// moment drinks it (with gulping sounds), which reveals the hidden key.
/// </summary>
public class Flask : MonoBehaviour
{
    [Tooltip("Glow and particles shown while the flask is full.")]
    [SerializeField] private GameObject potion;
    [Tooltip("How close any part of the flask has to be to the player's mouth.")]
    [SerializeField] private float mouthReach = 0.2f;
    [Tooltip("How long the flask has to stay at the mouth to drink it, in seconds.")]
    [SerializeField] private float sipDuration = 0.6f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private AudioSource audioSource;
    private AudioClip gulpClip;
    private AudioClip chimeClip;
    private float sipTime;
    private bool sipping;
    private Light potionLight;
    private float fullGlow;

    public bool IsFilled { get; private set; }

    private void Start()
    {
        if (potion != null)
            potion.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 0.3f;
        audioSource.volume = volume;
        gulpClip = PotionSounds.CreateGulps();
        chimeClip = PotionSounds.CreateChime();
    }

    public void Fill()
    {
        IsFilled = true;
        if (potion != null)
            potion.SetActive(true);
    }

    private void Update()
    {
        var game = EscapeRoomGame.Instance;
        if (!IsFilled || game == null || game.Stage != EscapeStage.DrinkPotion)
            return;

        Transform head = XRRigUtility.Head;
        if (head == null)
            return;

        if (IsAtMouth(head))
        {
            if (!sipping)
            {
                sipping = true;
                audioSource.clip = gulpClip;
                audioSource.Play();
            }

            sipTime += Time.deltaTime;
            SetGlow(1f - sipTime / sipDuration);
            if (sipTime >= sipDuration)
                Drink(game);
        }
        else
        {
            // Pulling the flask away interrupts the sip.
            if (sipping)
                audioSource.Stop();
            sipping = false;
            sipTime = 0f;
            SetGlow(1f);
        }
    }

    /// <summary>Dims the potion's glow as it is drunk, so the player can see the sip working.</summary>
    private void SetGlow(float amount)
    {
        if (potionLight == null && potion != null)
        {
            potionLight = potion.GetComponentInChildren<Light>(true);
            if (potionLight != null)
                fullGlow = potionLight.intensity;
        }
        if (potionLight != null)
            potionLight.intensity = fullGlow * Mathf.Clamp01(amount);
    }

    private bool IsAtMouth(Transform head)
    {
        // The mouth sits a little below and in front of the eyes.
        Vector3 mouth = head.position - head.up * 0.08f + head.forward * 0.03f;
        var flaskCollider = GetComponent<Collider>();
        Vector3 nearest = flaskCollider != null ? flaskCollider.bounds.ClosestPoint(mouth) : XRRigUtility.CenterOf(this);
        return Vector3.Distance(nearest, mouth) < mouthReach;
    }

    private void Drink(EscapeRoomGame game)
    {
        IsFilled = false;
        sipping = false;
        if (potion != null)
            potion.SetActive(false);

        audioSource.Stop();
        audioSource.PlayOneShot(chimeClip);
        game.SetStage(EscapeStage.FindKey);
    }
}
