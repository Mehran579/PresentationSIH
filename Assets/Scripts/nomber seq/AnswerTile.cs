using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnswerTile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private NumberDigitDisplay display;
    public int Value { get; private set; }
    public event Action<AnswerTile> OnTapped;

    public void Setup(int value)
    {
        Value = value;
        display.SetVariant(DigitVariant.Neutral);
        display.ShowNumber(value);
    }

    public void SetState(DigitVariant variant)
    {
        display.SetVariant(variant);
        display.ShowNumber(Value);
    }

    public void OnPointerClick(PointerEventData eventData) => OnTapped?.Invoke(this);
}