using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// Accepts the herbs, detects the wand being waved over it to cast the spell,
/// and fills the flask once the potion is enchanted.
/// </summary>
public class Cauldron : MonoBehaviour
{
    [Header("Puzzle pieces")]
    [SerializeField] private Herb[] herbs;
    [SerializeField] private Grabbable wand;
    [SerializeField] private Flask flask;

    [Header("Effects")]
    [SerializeField] private Renderer liquid;
    [SerializeField] private Light glow;
    [SerializeField] private ParticleSystem splash;
    [SerializeField] private ParticleSystem bubbles;
    [SerializeField] private ParticleSystem spellBurst;
    [SerializeField] private ParticleSystem wandSparkles;
    [SerializeField] private Color emptyColor = new Color(0.12f, 0.18f, 0.12f);
    [SerializeField] private Color brewColor = new Color(0.2f, 0.75f, 0.25f);
    [SerializeField] private Color spellColor = new Color(0.65f, 0.25f, 1f);

    [Header("Shape")]
    [Tooltip("Centre of the cauldron's mouth, at rim height.")]
    [SerializeField] private Transform mouth;
    [SerializeField] private float mouthRadius = 0.25f;
    [Tooltip("Extra reach beyond the opening, so dropping onto the rim still counts.")]
    [SerializeField] private float mouthMargin = 0.08f;
    [Tooltip("How far above the opening an item still counts as going in.")]
    [SerializeField] private float aboveMouthHeight = 0.25f;

    [Header("Spell")]
    [Tooltip("How far out from the cauldron the wand still counts as waving over it.")]
    [SerializeField] private float spellRadius = 0.9f;
    [Tooltip("How high above the cauldron the wand still counts as waving over it.")]
    [SerializeField] private float spellHeight = 1.2f;
    [Tooltip("Total distance, in metres, the wand has to be waved to cast the spell.")]
    [SerializeField] private float waveDistanceToCast = 1.2f;

    private float waved;
    private Vector3 lastWandPosition;
    private bool tracking;
    private float lastWaveTime;
    private Material liquidMaterial;

    private float SpellProgress => Mathf.Clamp01(waved / waveDistanceToCast);

    private void Start()
    {
        if (liquid != null)
            liquidMaterial = liquid.material;
        SetLiquid(emptyColor, 0f);
        SetWandSparkles(false);
        if (glow != null)
            glow.enabled = false;
    }

    private void Update()
    {
        var game = EscapeRoomGame.Instance;
        if (game == null || mouth == null)
            return;

        switch (game.Stage)
        {
            case EscapeStage.CollectHerbs:
                CheckHerbs(game);
                break;
            case EscapeStage.CastSpell:
                TrackWand(game);
                break;
            case EscapeStage.FillFlask:
                if (flask != null && IsInMouth(flask))
                {
                    flask.Fill();
                    if (splash != null)
                        splash.Play();
                    game.SetStage(EscapeStage.DrinkPotion);
                }
                break;
        }
    }

    private void CheckHerbs(EscapeRoomGame game)
    {
        foreach (var herb in herbs)
        {
            if (herb == null || !herb.gameObject.activeInHierarchy)
                continue;
            if (!IsInMouth(herb))
                continue;

            herb.gameObject.SetActive(false);
            if (splash != null)
                splash.Play();
            game.AddHerb();

            float progress = Mathf.Clamp01((float)game.HerbsAdded / game.HerbsRequired);
            SetLiquid(Color.Lerp(emptyColor, brewColor, progress), progress * 0.5f);
            if (game.Stage == EscapeStage.CastSpell && bubbles != null)
                bubbles.Play();
        }
    }

    /// <summary>
    /// Adds up how far the wand is waved anywhere near or above the cauldron. Any motion counts
    /// (circles, side to side, up and down); the brew turns purple as the spell builds up.
    /// </summary>
    private void TrackWand(EscapeRoomGame game)
    {
        if (wand == null)
            return;

        Vector3 position = XRRigUtility.CenterOf(wand);
        if (!tracking)
        {
            lastWandPosition = position;
            tracking = true;
            return;
        }

        float step = Vector3.Distance(position, lastWandPosition);
        lastWandPosition = position;
        float speed = step / Mathf.Max(Time.deltaTime, 0.0001f);

        // Ignore the wand sitting still or jittering, and big jumps from teleporting.
        bool waving = speed > 0.15f && speed < 8f && IsNearCauldron(wand);
        if (waving)
        {
            waved += step;
            lastWaveTime = Time.time;
            SetWandSparkles(true);
            SetLiquid(Color.Lerp(brewColor, spellColor, SpellProgress), 0.5f + SpellProgress * 1.5f);

            if (waved >= waveDistanceToCast)
                CastSpell(game);
        }
        else if (Time.time - lastWaveTime > 0.5f)
        {
            // Pausing doesn't lose the progress, it only fades slowly.
            SetWandSparkles(false);
            waved = Mathf.Max(0f, waved - Time.deltaTime * 0.2f);
        }
    }

    private bool IsNearCauldron(Component item)
    {
        Vector3 offset = BoundsOf(item).ClosestPoint(mouth.position) - mouth.position;
        float distance = new Vector2(offset.x, offset.z).magnitude;
        return distance < spellRadius
            && offset.y > -0.4f
            && offset.y < spellHeight;
    }

    private void CastSpell(EscapeRoomGame game)
    {
        SetWandSparkles(false);
        if (spellBurst != null)
            spellBurst.Play();
        SetLiquid(spellColor, 2f);
        if (glow != null)
        {
            glow.color = spellColor;
            glow.enabled = true;
        }
        game.SetStage(EscapeStage.FillFlask);
    }

    /// <summary>
    /// True when any part of the item reaches the space just above or inside the cauldron's
    /// opening, rim included, so a herb counts as soon as it touches the top.
    /// </summary>
    private bool IsInMouth(Component item)
    {
        Vector3 nearest = BoundsOf(item).ClosestPoint(mouth.position);
        Vector3 offset = nearest - mouth.position;
        float distance = new Vector2(offset.x, offset.z).magnitude;
        return distance < mouthRadius + mouthMargin
            && offset.y < aboveMouthHeight
            && offset.y > -0.6f;
    }

    private static Bounds BoundsOf(Component item)
    {
        var collider = item.GetComponent<Collider>();
        return collider != null ? collider.bounds : XRRigUtility.RendererBounds(item.transform);
    }

    private void SetLiquid(Color color, float emission)
    {
        if (liquidMaterial == null)
            return;

        liquidMaterial.SetColor("_BaseColor", color);
        liquidMaterial.EnableKeyword("_EMISSION");
        liquidMaterial.SetColor("_EmissionColor", color * emission);
    }

    private void SetWandSparkles(bool on)
    {
        if (wandSparkles == null)
            return;

        var emission = wandSparkles.emission;
        emission.enabled = on;
        if (on && !wandSparkles.isPlaying)
            wandSparkles.Play();
    }
}
