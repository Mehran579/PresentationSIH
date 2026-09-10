using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryUI : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text levelText;
    public TMP_Text pairsText;
    public TMP_Text matchesText;
    public TMP_Text revealText;
    public TMP_Text mismatchText;
    public TMP_Text sessionTimeText;

    [Header("Runtime Controls")]
    public Slider revealSlider;
    public Slider mismatchSlider;

    public void UpdateLevel(int level)
    {
        levelText.text = "Level: " + level;
    }

    public void UpdatePairs(int pairs)
    {
        pairsText.text = "Pairs: " + pairs;
    }

    public void UpdateMatches(int matches, int total)
    {
        matchesText.text = "Matches: " + matches + " / " + total;
    }

    public void UpdateRevealTime(float time)
    {
        revealText.text = "Reveal: " + time.ToString("0.0") + "s";

        if (revealSlider != null)
            revealSlider.SetValueWithoutNotify(time);
    }

    public void UpdateMismatchTime(float time)
    {
        mismatchText.text = "Mismatch: " + time.ToString("0.0") + "s";

        if (mismatchSlider != null)
            mismatchSlider.SetValueWithoutNotify(time);
    }

    public void UpdateSessionTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        sessionTimeText.text =
            "Session: " +
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }
}