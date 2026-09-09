using UnityEngine;
using UnityEngine.UI;
using PrimeTween;


public class card : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSpite;
    public Sprite iconSprite;

    public bool isSelected;

    public MemoryCard memoryCard;

    public void OnCardClick()
    {
        memoryCard.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }
    public void Show()
    {
        Tween.Rotation(transform, new Vector3(0, 180f, 0), 0.5f);
        Tween.Delay(0.1f, () => iconImage.sprite = iconSprite);
        isSelected = true;
    }
    public void Hide()
    {
        Tween.Rotation(transform, new Vector3(0, 0f, 0), 0.5f);
        Tween.Delay(0.1f, () => iconImage.sprite = hiddenIconSpite);

        isSelected = false;
    }
}
