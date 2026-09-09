using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MemoryCard : MonoBehaviour
{
    public card cardPrefab;
    public Transform gridTransform;
    public Sprite[] sprites;

    private List<Sprite> spritePairs;

    public int pairsToUse = 2;
    public float revealDuration;
    public float misMatchDelay;
    
    
    card firstselected;
    card secondselected;

    int matchcounts;

    private IEnumerator Start()
    {
        PrepareSprite();
        createcards();

        // Show all cards
        ShowAllCards();

        // Wait based on difficulty
        yield return new WaitForSeconds(revealDuration);

        // Hide all cards
        HideAllCards();
    }
    void ShowAllCards()
    {
        foreach (card c in gridTransform.GetComponentsInChildren<card>())
        {
            c.Show();
        }
    }

    void HideAllCards()
    {
        foreach (card c in gridTransform.GetComponentsInChildren<card>())
        {
            c.Hide();
        }
    }

    private void PrepareSprite()
    {
        spritePairs = new List<Sprite>();
        for (int i = 0; i < pairsToUse; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        shufflesprites(spritePairs);    
    }

    void createcards()
    {
        for(int i = 0; i < spritePairs.Count; i++)
        {
            card newCard = Instantiate(cardPrefab, gridTransform);
            newCard.SetIconSprite(spritePairs[i]);
            newCard.memoryCard = this;
        }
    }
    
    public void SetSelected(card card)
    {
        if (card.isSelected == false)
        {
            card.Show();
            if (firstselected == null) 
            {
                firstselected = card;
                return;
            }
            if (secondselected == null) 
            {
                secondselected = card;
                StartCoroutine(CheckMatching(firstselected, secondselected));
                firstselected = null;
                secondselected = null;
            }
        }
    }
    
    IEnumerator CheckMatching(card a, card b)
    {
        yield return new WaitForSeconds(0.3f);
        if(a.iconSprite == b.iconSprite)
        {
            //jeet gae
            Debug.Log("Matched");
            matchcounts++;
            if(matchcounts >= pairsToUse)
            {
                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one, 0.1f));
            }
        }
        else
        {
            a.Hide();
            b.Hide();
        }
    }
    
    void shufflesprites(List<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite temp = sprites[i];
            int randomIndex = Random.Range(i, sprites.Count);
            sprites[i] = sprites[randomIndex];
            sprites[randomIndex] = temp;
        }
    }
}
