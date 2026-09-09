using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class NumberSequence : MonoBehaviour
{
    public int level;

    [Header("Question Options")]
    public Image[] QuesImages;

    [Header("Question Sprites")]
    public Sprite[] level1sprites;
    public Sprite[] level2sprites;
    public Sprite[] level3sprites;
    public Sprite[] level4sprites;

    public int correctAnsIndex;

    [Header("Answer Options")]
    public Image[] ansImages;

    [Header("Answer Sprites")]
    public Sprite[] level1ansSprite;
    public Sprite[] level2ansSprite;
    public Sprite[] level3ansSprite;
    public Sprite[] level4ansSprite;


    private void OnEnable()
    {
        LoadLevel(level);
    }

    void LoadLevel(int level)
    {
        switch (level)
        {
            case 1:
                level1();
                break;
            case 2:
                level2();
                break;
            case 3:
                level3();
                break;
            case 4:
                level4();
                break;
            default:
                Debug.Log("Level not found");
                break;
        }
    }
    void level1()
    {
        correctAnsIndex = 0;
        for (int i = 0; i < QuesImages.Length; i++)
        {
            QuesImages[i].sprite = level1sprites[i];
        }
        for (int i = 0; i < ansImages.Length; i++)
        {
            ansImages[i].sprite = level1ansSprite[i];
        }
    }

    void level2()
    {
        correctAnsIndex = 2;
        for (int i = 0; i < QuesImages.Length; i++)
        {
            QuesImages[i].sprite = level2sprites[i];
        }
        for (int i = 0; i < ansImages.Length; i++)
        {
            ansImages[i].sprite = level2ansSprite[i];
        }
    }

    void level3()
    {
        correctAnsIndex = 1;
        for (int i = 0; i < QuesImages.Length; i++)
        {
            QuesImages[i].sprite = level3sprites[i];
        }
        for (int i = 0; i < ansImages.Length; i++)
        {
            ansImages[i].sprite = level3ansSprite[i];
        }
    }

    void level4()
    {
        correctAnsIndex = 3;
        for (int i = 0; i < QuesImages.Length; i++)
        {
            QuesImages[i].sprite = level4sprites[i];
        }
        for (int i = 0; i < ansImages.Length; i++)
        {
            ansImages[i].sprite = level4ansSprite[i];
        }
    }
    public RectTransform answergrid;
    public void OnClicked(int buttonIndex)
    {
        if (buttonIndex == correctAnsIndex)
        {
            Debug.Log("Correct Answer!");
            level++;
            LoadLevel(level);
        }
        else
        {
            Debug.Log("Wrong Answer!");

            Tween.ShakeLocalPosition(
                answergrid,
                new Vector3(10, 10),
                0.3f,
                10
            );
        }
    }
}
