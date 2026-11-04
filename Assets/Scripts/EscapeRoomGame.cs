using System;
using UnityEngine;

public enum EscapeStage
{
    CollectHerbs,
    CastSpell,
    FillFlask,
    DrinkPotion,
    FindKey,
    UnlockDoor,
    Escaped
}

/// <summary>
/// Tracks the escape room's progress and shows the current goal on the hint board.
/// </summary>
public class EscapeRoomGame : MonoBehaviour
{
    public static EscapeRoomGame Instance { get; private set; }

    [SerializeField] private int herbsRequired = 3;
    [SerializeField] private TextMesh hintText;
    [Tooltip("Appears when the potion is drunk.")]
    [SerializeField] private HiddenItem hiddenKey;

    public EscapeStage Stage { get; private set; } = EscapeStage.CollectHerbs;
    public int HerbsAdded { get; private set; }
    public int HerbsRequired => herbsRequired;

    public event Action<EscapeStage> StageChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        RefreshHint();
    }

    public void AddHerb()
    {
        if (Stage != EscapeStage.CollectHerbs)
            return;

        HerbsAdded++;
        if (HerbsAdded >= herbsRequired)
            SetStage(EscapeStage.CastSpell);
        else
            RefreshHint();
    }

    /// <summary>Moves the game forward to the given stage. Going backwards is ignored.</summary>
    public void SetStage(EscapeStage stage)
    {
        if (stage <= Stage)
            return;

        Stage = stage;
        Debug.Log($"[Escape Room] Stage: {stage}");
        if (stage == EscapeStage.FindKey)
        {
            if (hiddenKey != null)
                hiddenKey.Reveal();
            else
                Debug.LogWarning("[Escape Room] No hidden key is assigned on the Game object.");
        }

        RefreshHint();
        StageChanged?.Invoke(stage);
    }

    private void RefreshHint()
    {
        if (hintText != null)
            hintText.text = HintFor(Stage) + "\n\n<size=40><color=#b0a890>Hold the menu button to restart</color></size>";
    }

    private string HintFor(EscapeStage stage)
    {
        switch (stage)
        {
            case EscapeStage.CollectHerbs:
                return $"Find the {herbsRequired} herbs in the rooms\nof the maze and drop them\ninto the cauldron  ({HerbsAdded}/{herbsRequired})";
            case EscapeStage.CastSpell:
                return "The brew is ready!\nGrab the wand and wave it\nover the cauldron";
            case EscapeStage.FillFlask:
                return "The potion is enchanted!\nDip the flask into the cauldron";
            case EscapeStage.DrinkPotion:
                return "The flask is full!\nHold it to your mouth\nto drink the potion";
            case EscapeStage.FindKey:
                return "A glowing key has appeared\non the table by the door!";
            case EscapeStage.UnlockDoor:
                return "Touch the key to the door\nto unlock it";
            default:
                return "The door is open!\nWalk through it to escape";
        }
    }
}
