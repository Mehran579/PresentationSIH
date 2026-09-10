using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

namespace SmritiCare.PatternRecall
{
    /// <summary>One round's outcome — enough to log, nothing level-system-specific.</summary>
    [Serializable]
    public class PatternRecallRoundResult
    {
        public int gridSize;
        public int cellsToHighlight;
        public int correctSelections;   // pattern cells the player tapped
        public int missedCells;         // pattern cells the player did NOT tap
        public int incorrectSelections; // non-pattern cells the player tapped
        public float responseTimeSeconds; // reveal-end (or highlight-start if untimed) -> Check pressed
    }

    public class PatternRecallController : MonoBehaviour
    {
        [Header("Scene refs")]
        [SerializeField] private RectTransform gridContainer;   // has GridLayoutGroup on it
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private GameObject tilePrefab;         // prefab with PatternTile
        [SerializeField] private Button checkButton;

        [Header("Round settings — plug a real level system into these later")]
        [Tooltip("Grid is gridSize x gridSize.")]
        [Range(2, 4)]
        [SerializeField] private int gridSize = 2;

        [Tooltip("How many tiles light up this round.")]
        [Min(1)]
        [SerializeField] private int cellsToHighlight = 1;

        [Tooltip("How long the pattern stays lit before it's hidden, in seconds. Ignored if untimed is on.")]
        [Min(0.5f)]
        [SerializeField] private float revealDuration = 3f;

        [Tooltip("If true, the pattern stays visible while the player taps (near-untimed, " +
                 "'confirm you understand' round). If false, pattern hides before the player can respond. " +
                 "Stand-in for real level data — wire this up to your level system later.")]
        [SerializeField] private bool isUntimedLevel = false;

        [Header("Pacing")]
        [SerializeField] private float interRoundPause = 1.5f;
        [SerializeField] private float feedbackHoldDuration = 2f;



        [Header("Level System")]
        [SerializeField] private int level = 1;
        [SerializeField] private int roundsPerLevel = 1;

        [SerializeField] private float revealDecreasePerLevel = 0.5f;
        [SerializeField] private int extraTilesPerLevel = 1;

        /// <summary>Fires after each round's feedback is scored, before the next round starts.</summary>
        public event Action<PatternRecallRoundResult> OnRoundComplete;

        private readonly List<PatternTile> _activeTiles = new List<PatternTile>();
        private readonly HashSet<int> _patternIndices = new HashSet<int>();
        private readonly HashSet<int> _selectedIndices = new HashSet<int>();

        private float _roundClockStartTime; // reset at reveal-end (or highlight-start if untimed)
        private bool _waitingForCheck;
        private Coroutine _playLoop;

        // ---------- Hub integration ----------

        /// <summary>Call when the hub switches into this game.</summary>
        private void Start()
        {
            Activate();
        }
        public void Activate()
        {
            gameObject.SetActive(true);
            checkButton.onClick.AddListener(OnCheckPressed);
            _playLoop = StartCoroutine(PlayContinuously());
        }

        /// <summary>Call when the hub switches away — stops everything and clears the grid.</summary>
        public void Deactivate()
        {
            if (_playLoop != null) StopCoroutine(_playLoop);
            checkButton.onClick.RemoveListener(OnCheckPressed);
            ClearGrid();
            gameObject.SetActive(false);
        }

        // ---------- Round flow ----------

        private IEnumerator PlayContinuously()
        {
            checkButton.interactable = false;
            while (true)
            {
                yield return RunRound();
                yield return new WaitForSeconds(interRoundPause);
            }
        }

        private IEnumerator RunRound()
        {
            SpawnGrid();
            PickPatternIndices();

            foreach (var index in _patternIndices)
                _activeTiles[index].SetState(TileState.Highlighted);

            if (!isUntimedLevel)
            {
                // Normal flow: show it, hide it, then let the player answer from memory.
                yield return new WaitForSeconds(revealDuration);

                foreach (var tile in _activeTiles)
                    tile.SetState(TileState.Idle);

                _roundClockStartTime = Time.realtimeSinceStartup;
                EnableInput(true);
            }
            else
            {
                // Untimed: pattern stays lit, player taps while it's still visible.
                // Clock starts immediately since there's no hide event to anchor to.
                _roundClockStartTime = Time.realtimeSinceStartup;
                EnableInput(true);
            }

            checkButton.interactable = true;
            _waitingForCheck = true;

            // No timer on the response itself — just wait for Check.
            yield return new WaitUntil(() => !_waitingForCheck);

            var result = ScoreRound();
            OnRoundComplete?.Invoke(result);
            IncreaseLevel();
            Debug.Log($"[PatternRecall] round done — grid {result.gridSize}x{result.gridSize}, " +
                      $"correct {result.correctSelections}, missed {result.missedCells}, " +
                      $"incorrect {result.incorrectSelections}, response {result.responseTimeSeconds:F1}s");

            yield return ShowFeedback();
        }

        private void SpawnGrid()
        {
            ClearGrid();

            gridLayoutGroup.constraintCount = gridSize;
            int totalTiles = gridSize * gridSize;

            for (int i = 0; i < totalTiles; i++)
            {
                var go = Instantiate(tilePrefab, gridContainer);
                var tile = go.GetComponent<PatternTile>();
                tile.Init(i);
                tile.OnTileTapped += HandleTileTapped;
                _activeTiles.Add(tile);
            }
        }

        private void ClearGrid()
        {
            foreach (var tile in _activeTiles)
            {
                tile.OnTileTapped -= HandleTileTapped;
                Destroy(tile.gameObject);
            }
            _activeTiles.Clear();
            _patternIndices.Clear();
            _selectedIndices.Clear();
        }

        private void PickPatternIndices()
        {
            _patternIndices.Clear();
            int totalTiles = gridSize * gridSize;
            var pool = new List<int>(totalTiles);
            for (int i = 0; i < totalTiles; i++) pool.Add(i);

            // Fisher-Yates partial shuffle — pick cellsToHighlight unique indices.
            int count = Mathf.Min(cellsToHighlight, pool.Count);
            for (int i = 0; i < count; i++)
            {
                int r = UnityEngine.Random.Range(0, pool.Count);
                _patternIndices.Add(pool[r]);
                pool.RemoveAt(r);
            }
        }

        private void EnableInput(bool enabled)
        {
            foreach (var tile in _activeTiles)
                tile.InputEnabled = enabled;
        }

        // ---------- Input handling ----------

        private void HandleTileTapped(PatternTile tile)
        {
            // Toggle selection so the player can change their mind before Check.
            if (_selectedIndices.Contains(tile.Index))
            {
                _selectedIndices.Remove(tile.Index);
                // Restore whatever it should look like right now (still-lit if untimed, else idle).
                bool stillShowingHighlight = isUntimedLevel && _patternIndices.Contains(tile.Index);
                tile.SetState(stillShowingHighlight ? TileState.Highlighted : TileState.Idle);
            }
            else
            {
                _selectedIndices.Add(tile.Index);
                tile.SetState(TileState.Selected);
            }
        }

        private void OnCheckPressed()
        {
            if (!_waitingForCheck) return;
            EnableInput(false);
            checkButton.interactable = false;
            _waitingForCheck = false;
        }

        // ---------- Scoring & feedback ----------

        private PatternRecallRoundResult ScoreRound()
        {
            int correct = 0, missed = 0, incorrect = 0;

            foreach (var tile in _activeTiles)
            {
                bool isPattern = _patternIndices.Contains(tile.Index);
                bool isSelected = _selectedIndices.Contains(tile.Index);

                if (isPattern && isSelected) { correct++; tile.SetState(TileState.Correct); }
                else if (isPattern && !isSelected) { missed++; tile.SetState(TileState.MissedCorrect); }
                else if (!isPattern && isSelected) { incorrect++; tile.SetState(TileState.IncorrectSelected); }
                // !isPattern && !isSelected -> stays Idle, nothing to show
            }

            return new PatternRecallRoundResult
            {
                gridSize = gridSize,
                cellsToHighlight = cellsToHighlight,
                correctSelections = correct,
                missedCells = missed,
                incorrectSelections = incorrect,
                responseTimeSeconds = Time.realtimeSinceStartup - _roundClockStartTime
            };
        }

        private IEnumerator ShowFeedback()
        {
            yield return new WaitForSeconds(feedbackHoldDuration);
            foreach (var tile in _activeTiles)
                tile.SetState(TileState.Idle);
        }

        private void IncreaseLevel()
        {
            level++;

            gridSize = Mathf.Min(4, 2 + (level - 1) / 2);
            cellsToHighlight = Mathf.Min(gridSize * gridSize - 1, 1 + (level - 1));
            revealDuration = Mathf.Max(1f, 3f - (level - 1) * revealDecreasePerLevel);

            Debug.Log($"[PatternRecall] Level {level} — Grid: {gridSize}x{gridSize}, " +
                      $"Tiles: {cellsToHighlight}, Reveal: {revealDuration:F1}s");
        }
    }
}