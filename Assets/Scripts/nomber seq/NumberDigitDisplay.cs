using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NumberDigitDisplay : MonoBehaviour
{
    [SerializeField] private Image digitPrefab;
    [SerializeField] private Transform digitParent;
    [SerializeField] private DigitSpriteSet[] variantSets;

    private DigitSpriteSet _currentSet;
    private List<GameObject> _activeDigits = new List<GameObject>();

    public void SetVariant(DigitVariant variant)
    {
        _currentSet = null;
        foreach (var set in variantSets)
        {
            if (set != null && set.variant == variant)
            {
                _currentSet = set;
                return;
            }
        }
        Debug.LogError($"[!] No DigitSpriteSet found for variant {variant} on {gameObject.name}. Check the Variant Sets array in the Inspector!");
    }

    public void ShowNumber(int value)
    {
        ClearDigits();

        if (_currentSet == null)
        {
            Debug.LogError($"[!] Cannot show number {value} on {gameObject.name} because no SpriteSet is assigned!");
            return;
        }

        if (_currentSet.digits == null || _currentSet.digits.Length < 10)
        {
            Debug.LogError($"[!] SpriteSet for {gameObject.name} has a null or incomplete digits array! Must have 10 sprites.");
            return;
        }

        string text = value.ToString();
        foreach (char c in text)
        {
            int d = c - '0';
            if (d < 0 || d > 9) continue;

            Sprite s = _currentSet.digits[d];
            if (s == null)
            {
                Debug.LogError($"[!] Sprite for digit {d} is missing in the SpriteSet for {gameObject.name}!");
                continue;
            }

            var img = Instantiate(digitPrefab, digitParent);
            img.sprite = s;
            img.gameObject.SetActive(true);
            _activeDigits.Add(img.gameObject);
        }
    }

    public void ShowBlank()
    {
        ClearDigits();

        if (_currentSet == null)
        {
            Debug.LogError($"[!] Cannot show blank on {gameObject.name} because no SpriteSet is assigned!");
            return;
        }

        if (_currentSet.blank == null)
        {
            Debug.LogError($"[!] Blank sprite is missing in the SpriteSet for {gameObject.name}!");
            return;
        }

        var img = Instantiate(digitPrefab, digitParent);
        img.sprite = _currentSet.blank;
        img.gameObject.SetActive(true);
        _activeDigits.Add(img.gameObject);
    }

    private void ClearDigits()
    {
        foreach (var digit in _activeDigits)
        {
            if (digit != null) Destroy(digit);
        }
        _activeDigits.Clear();

        for (int i = digitParent.childCount - 1; i >= 0; i--)
        {
            Destroy(digitParent.GetChild(i).gameObject);
        }
    }
}