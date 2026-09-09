using UnityEngine;

public enum DigitVariant { Neutral, Selected, Correct, Highlighted }

[CreateAssetMenu(menuName = "SmritiCare/NumberSequence/DigitSpriteSet")]
public class DigitSpriteSet : ScriptableObject
{
    public DigitVariant variant;
    public Sprite[] digits = new Sprite[10];
    public Sprite blank;
}