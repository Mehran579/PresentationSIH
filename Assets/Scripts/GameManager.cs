using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject[] games;

    private int currentGame = 0;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetActiveGame(0);
    }

    public void SetActiveGame(int index)
    {
        if (index < 0 || index >= games.Length)
            return;

        currentGame = index;

        // Only the selected game is active
        for (int i = 0; i < games.Length; i++)
        {
            games[i].SetActive(i == currentGame);
        }
    }

    public void NextGame()
    {
        SetActiveGame(currentGame + 1);
    }

    public void PreviousGame()
    {
        SetActiveGame(currentGame - 1);
    }
    public void back()
    {
        SetActiveGame(0);
    }
}