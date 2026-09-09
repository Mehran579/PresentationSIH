using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmritiCare.PatternRecall
{
    /// <summary>
    /// Visual states a tile can be put into. The tile itself has zero opinion
    /// about which of these is "right" — the controller decides and calls SetState.
    /// </summary>
    public enum TileState
    {
        Idle,             // blank / neutral, not part of anything right now
        Highlighted,      // showing as part of the pattern during the reveal
        Selected,         // player has tapped it, awaiting Check
        Correct,          // confirmed correct after Check pressed
        IncorrectSelected,// player tapped it but it wasn't in the pattern (soft amber, never red)
        MissedCorrect     // was in the pattern but player didn't tap it (gentle, non-punishing reveal)
    }

    /// <summary>
    /// One cell in the grid. Deliberately "dumb": it only knows its own index,
    /// how to change color for a given state, and how to report a tap.
    /// All game logic (what's correct, what phase we're in, when input is allowed)
    /// lives in PatternRecallController.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PatternTile : MonoBehaviour, IPointerClickHandler
    {
        [Header("Single consistent color set — reused across every level, never swapped per level")]
        [SerializeField] private Color idleColor = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color highlightColor = new Color(0.30f, 0.70f, 0.45f);
        [SerializeField] private Color selectedColor = new Color(0.55f, 0.60f, 0.90f);
        [SerializeField] private Color correctColor = new Color(0.30f, 0.70f, 0.45f); // same family as highlight
        [SerializeField] private Color incorrectColor = new Color(0.90f, 0.82f, 0.55f); // soft amber, not red
        [SerializeField] private Color missedCorrectColor = new Color(0.65f, 0.85f, 0.68f); // faint green, "here's where it was"

        public int Index { get; private set; }
        public TileState CurrentState { get; private set; } = TileState.Idle;

        /// <summary>Controller flips this on/off per phase — tile does no phase logic itself.</summary>
        public bool InputEnabled { get; set; } = false;

        /// <summary>Fired on every accepted tap. Controller decides what happens next.</summary>
        public event Action<PatternTile> OnTileTapped;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        /// <summary>Called by the controller right after Instantiate, once per round-setup.</summary>
        public void Init(int index)
        {
            Index = index;
            InputEnabled = false;
            SetState(TileState.Idle);
        }

        public void SetState(TileState state)
        {
            CurrentState = state;
            _image.color = state switch
            {
                TileState.Idle => idleColor,
                TileState.Highlighted => highlightColor,
                TileState.Selected => selectedColor,
                TileState.Correct => correctColor,
                TileState.IncorrectSelected => incorrectColor,
                TileState.MissedCorrect => missedCorrectColor,
                _ => idleColor
            };
        }

        // IPointerClickHandler — works out of the box with InputSystemUIInputModule,
        // no new-Input-System-specific code needed here.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!InputEnabled) return;
            OnTileTapped?.Invoke(this);
        }
    }
}