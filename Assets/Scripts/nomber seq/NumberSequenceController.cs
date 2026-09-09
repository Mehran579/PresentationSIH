using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SequencePattern { Simple, Large, Multiplication, Mixed }

public class NumberSequenceController : MonoBehaviour
{
    private static NumberSequenceController _instance;
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[!] Duplicate controller detected on {gameObject.name}. Destroying to prevent bugs.");
            Destroy(this);
            return;
        }
        _instance = this;
    }

    [SerializeField] private NumberDigitDisplay[] sequenceTiles;
    [SerializeField] private AnswerTile[] answerTiles;
    [SerializeField] private SequencePattern currentPattern = SequencePattern.Simple;

    private int _correctAnswer;
    private float _revealTime;

    void Start()
    {
        ValidateReferences();
        StartRound();
    }

    private void ValidateReferences()
    {
        HashSet<NumberDigitDisplay> seenSeq = new HashSet<NumberDigitDisplay>();
        foreach (var tile in sequenceTiles)
        {
            if (tile == null) { Debug.LogError("SequenceTile is null!"); continue; }
            if (!seenSeq.Add(tile)) { Debug.LogError($"Duplicate sequence tile found: {tile.name} is assigned multiple times!"); }
        }

        HashSet<AnswerTile> seenAns = new HashSet<AnswerTile>();
        foreach (var tile in answerTiles)
        {
            if (tile == null) { Debug.LogError("AnswerTile is null!"); continue; }
            if (!seenAns.Add(tile)) { Debug.LogError($"Duplicate answer tile found: {tile.name} is assigned multiple times!"); }
        }
    }

    public void StartRound()

    {
        StopAllCoroutines();
        StartCoroutine(RunRound());
    }

    private IEnumerator RunRound()
    {
        var (sequence, blankValue) = GenerateSequence();
        _correctAnswer = blankValue;

        for (int i = 0; i < sequenceTiles.Length; i++)
        {
            bool isActive = i < 4;
            sequenceTiles[i].gameObject.SetActive(isActive);
            if (!isActive) continue;

            sequenceTiles[i].SetVariant(DigitVariant.Neutral);
            if (i < 3) sequenceTiles[i].ShowNumber(sequence[i]);
            else sequenceTiles[i].ShowBlank();
        }

        var choices = GenerateChoices(_correctAnswer, sequence);
        for (int i = 0; i < answerTiles.Length; i++)
        {
            answerTiles[i].gameObject.SetActive(i < choices.Count);
            if (i >= choices.Count) continue;

            answerTiles[i].OnTapped -= HandleTapped;
            answerTiles[i].Setup(choices[i]);
            answerTiles[i].OnTapped += HandleTapped;
        }

        _revealTime = Time.time;
        yield return null;
    }

    private void HandleTapped(AnswerTile tile)
    {
        bool correct = (tile.Value == _correctAnswer);
        tile.SetState(correct ? DigitVariant.Correct : DigitVariant.Selected);

        foreach (var a in answerTiles) a.OnTapped -= HandleTapped;

        StartCoroutine(NextRoundDelay(1.5f));
    }

    private IEnumerator NextRoundDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartRound();
    }

    private (List<int> seq, int blank) GenerateSequence()
    {
        int start = UnityEngine.Random.Range(1, 10);
        List<int> seq = new List<int>();
        int blank = 0;

        switch (currentPattern)
        {
            case SequencePattern.Simple:
                for (int i = 0; i < 3; i++) seq.Add(start + i);
                blank = start + 3;
                break;
            case SequencePattern.Large:
                int step = new[] { 5, 10, 25 }[UnityEngine.Random.Range(0, 3)];
                for (int i = 0; i < 3; i++) seq.Add(start * step + i * step);
                blank = start * step + 3 * step;
                break;
            case SequencePattern.Multiplication:
                int mult = UnityEngine.Random.Range(2, 4);
                int val = start;
                for (int i = 0; i < 3; i++) { seq.Add(val); val *= mult; }
                blank = val;
                break;
            case SequencePattern.Mixed:
                int altStart = start;
                seq.Add(altStart);
                seq.Add(altStart + 1);
                seq.Add(altStart + 3);
                blank = altStart + 4;
                break;
        }
        return (seq, blank);
    }

    private List<int> GenerateChoices(int correct, List<int> seq)
    {
        List<int> choices = new List<int> { correct };
        while (choices.Count < 3)
        {
            int off = UnityEngine.Random.Range(-3, 4);
            int candidate = correct + off;
            if (candidate > 0 && !choices.Contains(candidate) && !seq.Contains(candidate))
                choices.Add(candidate);
        }
        for (int i = choices.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (choices[i], choices[j]) = (choices[j], choices[i]);
        }
        return choices;
    }
}