using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryCard : MonoBehaviour
{
    [Header("Card Setup")]
    public card cardPrefab;
    public Transform gridTransform;
    public Sprite[] sprites;

    [Header("Difficulty")]
    public int level = 1;
    public int pairsToUse = 2;
    public int maxPairs = 12;

    [Header("Timing")]
    public float revealDuration = 2f;
    public float misMatchDelay = 0.3f;

    [Header("Grid")]
    public float gridPadding = 20f;
    public float maxSpacing = 25f;

    [Header("UI")]
    public MemoryUI memoryUI;

    private List<Sprite> spritePairs;

    private card firstSelected;
    private card secondSelected;

    private int matchCounts;

    private bool canSelect;
    private bool sessionStarted;

    private float sessionTime;

    private Coroutine levelCoroutine;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Start()
    {
        sessionStarted = true;

        UpdateUI();
    }

    private void OnEnable()
    {
        // Start/restart the current level whenever
        // SmritiCare enables this game.
        levelCoroutine = StartCoroutine(StartLevel());
    }


    // =========================================================
    // UPDATE / SESSION TIMER
    // =========================================================

    private void Update()
    {
        if (!sessionStarted)
            return;

        sessionTime += Time.deltaTime;

        if (memoryUI != null)
            memoryUI.UpdateSessionTime(sessionTime);
    }


    // =========================================================
    // LEVEL
    // =========================================================

    private IEnumerator StartLevel()
    {
        canSelect = false;

        firstSelected = null;
        secondSelected = null;

        matchCounts = 0;

        pairsToUse = Mathf.Clamp(
            pairsToUse,
            1,
            maxPairs
        );

        CreateBoard();

        UpdateUI();

        // Show all cards at the beginning.
        ShowAllCards();

        yield return new WaitForSeconds(revealDuration);

        // Hide all cards.
        HideAllCards();

        // Player can now play.
        canSelect = true;

        UpdateUI();

        levelCoroutine = null;
    }


    // =========================================================
    // CREATE BOARD
    // =========================================================

    private void CreateBoard()
    {
        // Destroy cards from the previous level.
        for (int i = gridTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridTransform.GetChild(i).gameObject);
        }

        PrepareSprites();
        CreateCards();
        UpdateGridLayout();
    }


    // =========================================================
    // SPRITES
    // =========================================================

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();

        for (int i = 0; i < pairsToUse; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }

        ShuffleSprites(spritePairs);
    }


    private void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            card newCard = Instantiate(
                cardPrefab,
                gridTransform
            );

            newCard.SetIconSprite(spritePairs[i]);

            newCard.memoryCard = this;
        }
    }


    // =========================================================
    // DYNAMIC GRID
    // =========================================================

    private void UpdateGridLayout()
    {
        GridLayoutGroup grid =
            gridTransform.GetComponent<GridLayoutGroup>();

        if (grid == null)
        {
            Debug.LogError(
                "MemoryCard: GridTransform needs a GridLayoutGroup."
            );

            return;
        }

        RectTransform gridRect =
            gridTransform.GetComponent<RectTransform>();

        int totalCards = pairsToUse * 2;

        // Find a roughly square layout.
        int columns = Mathf.CeilToInt(
            Mathf.Sqrt(totalCards)
        );

        int rows = Mathf.CeilToInt(
            (float)totalCards / columns
        );


        // Available area inside the Grid.
        float availableWidth =
            gridRect.rect.width
            - gridPadding * 2f;

        float availableHeight =
            gridRect.rect.height
            - gridPadding * 2f;


        // Start with the maximum spacing.
        float spacingX = maxSpacing;
        float spacingY = maxSpacing;


        // Calculate card size based on available space.
        float cardWidth =
            (
                availableWidth
                - spacingX * (columns - 1)
            ) / columns;

        float cardHeight =
            (
                availableHeight
                - spacingY * (rows - 1)
            ) / rows;


        // Cards are square.
        float cardSize =
            Mathf.Min(cardWidth, cardHeight);


        // Prevent invalid/negative sizes.
        cardSize = Mathf.Max(
            cardSize,
            20f
        );


        // Calculate how much spacing actually fits.
        float requiredWidth =
            cardSize * columns;

        float requiredHeight =
            cardSize * rows;


        if (columns > 1)
        {
            spacingX = Mathf.Max(
                0f,
                (
                    availableWidth
                    - requiredWidth
                ) / (columns - 1)
            );

            spacingX = Mathf.Min(
                spacingX,
                maxSpacing
            );
        }
        else
        {
            spacingX = 0f;
        }


        if (rows > 1)
        {
            spacingY = Mathf.Max(
                0f,
                (
                    availableHeight
                    - requiredHeight
                ) / (rows - 1)
            );

            spacingY = Mathf.Min(
                spacingY,
                maxSpacing
            );
        }
        else
        {
            spacingY = 0f;
        }


        // Apply GridLayoutGroup settings.
        grid.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        grid.constraintCount = columns;

        grid.cellSize =
            new Vector2(
                cardSize,
                cardSize
            );

        grid.spacing =
            new Vector2(
                spacingX,
                spacingY
            );

        grid.padding.left =
            Mathf.RoundToInt(gridPadding);

        grid.padding.right =
            Mathf.RoundToInt(gridPadding);

        grid.padding.top =
            Mathf.RoundToInt(gridPadding);

        grid.padding.bottom =
            Mathf.RoundToInt(gridPadding);
    }


    // =========================================================
    // CARD SELECTION
    // =========================================================

    public void SetSelected(card selectedCard)
    {
        // Game isn't ready for input.
        if (!canSelect)
            return;

        // Card is already revealed/matched.
        if (selectedCard.isSelected)
            return;


        selectedCard.Show();


        // First card.
        if (firstSelected == null)
        {
            firstSelected = selectedCard;
            return;
        }


        // Second card.
        secondSelected = selectedCard;

        canSelect = false;

        StartCoroutine(
            CheckMatching(
                firstSelected,
                secondSelected
            )
        );
    }


    // =========================================================
    // MATCH CHECK
    // =========================================================

    private IEnumerator CheckMatching(card a, card b)
    {
        yield return new WaitForSeconds(
            misMatchDelay
        );


        if (a.iconSprite == b.iconSprite)
        {
            // MATCH
            Debug.Log("Matched");

            matchCounts++;


            // Keep both cards selected.
            // Your card.cs leaves isSelected = true,
            // so they cannot be selected again.

            firstSelected = null;
            secondSelected = null;


            // Check if the level is complete.
            if (matchCounts >= pairsToUse)
            {
                yield return StartCoroutine(
                    LevelWon()
                );
            }
            else
            {
                canSelect = true;
            }
        }
        else
        {
            // MISMATCH
            a.Hide();
            b.Hide();

            firstSelected = null;
            secondSelected = null;

            canSelect = true;
        }


        UpdateUI();
    }


    // =========================================================
    // LEVEL WON
    // =========================================================

    private IEnumerator LevelWon()
    {
        canSelect = false;


        Debug.Log(
            "Level " + level + " complete!"
        );


        // Win animation.
        PrimeTween.Sequence.Create()
            .Chain(
                PrimeTween.Tween.Scale(
                    gridTransform,
                    Vector3.one * 1.2f,
                    0.2f,
                    ease: PrimeTween.Ease.OutBack
                )
            )
            .Chain(
                PrimeTween.Tween.Scale(
                    gridTransform,
                    Vector3.one,
                    0.1f
                )
            );


        UpdateUI();


        yield return new WaitForSeconds(0.5f);


        // Increase level.
        level++;


        // Increase number of pairs.
        if (pairsToUse < maxPairs)
        {
            pairsToUse++;
        }


        Debug.Log(
            "Starting Level " +
            level +
            " with " +
            pairsToUse +
            " pairs."
        );


        // Start next level.
        levelCoroutine =
            StartCoroutine(StartLevel());

        yield return levelCoroutine;
    }


    // =========================================================
    // SHOW / HIDE CARDS
    // =========================================================

    private void ShowAllCards()
    {
        foreach (
            card c in
            gridTransform.GetComponentsInChildren<card>()
        )
        {
            c.Show();
        }
    }


    private void HideAllCards()
    {
        foreach (
            card c in
            gridTransform.GetComponentsInChildren<card>()
        )
        {
            c.Hide();
        }
    }


    // =========================================================
    // RUNTIME SETTINGS
    // =========================================================

    public void IncreasePairs()
    {
        SetPairs(
            pairsToUse + 1
        );
    }


    public void DecreasePairs()
    {
        SetPairs(
            pairsToUse - 1
        );
    }


    public void SetPairs(int amount)
    {
        pairsToUse =
            Mathf.Clamp(
                amount,
                1,
                maxPairs
            );

        RestartCurrentLevel();
    }


    public void SetRevealDuration(float value)
    {
        revealDuration =
            Mathf.Max(
                0.1f,
                value
            );

        UpdateUI();
    }


    public void SetMismatchDelay(float value)
    {
        misMatchDelay =
            Mathf.Max(
                0f,
                value
            );

        UpdateUI();
    }


    private void RestartCurrentLevel()
    {
        // Stop only the level coroutine.
        if (levelCoroutine != null)
        {
            StopCoroutine(levelCoroutine);
            levelCoroutine = null;
        }


        firstSelected = null;
        secondSelected = null;

        canSelect = false;


        levelCoroutine =
            StartCoroutine(StartLevel());


        UpdateUI();
    }


    // =========================================================
    // UI
    // =========================================================

    private void UpdateUI()
    {
        if (memoryUI == null)
            return;


        memoryUI.UpdateLevel(level);

        memoryUI.UpdatePairs(pairsToUse);

        memoryUI.UpdateMatches(
            matchCounts,
            pairsToUse
        );

        memoryUI.UpdateRevealTime(
            revealDuration
        );

        memoryUI.UpdateMismatchTime(
            misMatchDelay
        );

        memoryUI.UpdateSessionTime(
            sessionTime
        );
    }


    // =========================================================
    // SHUFFLE
    // =========================================================

    private void ShuffleSprites(
        List<Sprite> list
    )
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];

            int randomIndex =
                Random.Range(
                    i,
                    list.Count
                );

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }
}