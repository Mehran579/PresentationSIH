// Assets/Games/NERMemoryQuiz/NERQuizController.cs
//
// Simple version: show question + image + option buttons.
// Tap the right answer -> next question.
// Tap the wrong answer -> stay on this question, try again.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmritiCare.Games.NERMemoryQuiz
{
    // One question, matches your content JSON
    [System.Serializable]
    public class QuizItem
    {
        public string question;
        public string[] options;
        public int correctOption; // index into options[] that is correct
        public string imageUrl;
    }

    // Wrapper because your JSON has { "items": [ ...QuizItem... ] } at the top
    [System.Serializable]
    public class QuizContent
    {
        public QuizItem[] items;
    }

    public class NERQuizController : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TextAsset contentJson; // drag your JSON file in here

        [Header("UI")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private Image promptImage;
        [SerializeField] private Button[] optionButtons; // e.g. 4 buttons, drag in from your scene
        [SerializeField] private TMP_Text[] optionLabels; // same order/size as optionButtons

        private List<QuizItem> _items;
        private int _currentIndex;

        private void Start()
        {
            QuizContent parsed = JsonUtility.FromJson<QuizContent>(contentJson.text);
            _items = new List<QuizItem>(parsed.items);
            _currentIndex = 0;
            ShowQuestion(_items[_currentIndex]);
        }

        private void ShowQuestion(QuizItem item)
        {
            questionText.text = item.question;

            if (!string.IsNullOrEmpty(item.imageUrl))
            {
                promptImage.sprite = Resources.Load<Sprite>($"Images/{item.imageUrl}");
                promptImage.gameObject.SetActive(true);
            }
            else
            {
                promptImage.gameObject.SetActive(false);
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                bool hasOption = i < item.options.Length;
                optionButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption) continue;

                optionLabels[i].text = item.options[i];

                int optionIndex = i; // must copy the loop variable for the listener below
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnAnswerTapped(optionIndex));
            }
        }

        private void OnAnswerTapped(int tappedIndex)
        {
            QuizItem current = _items[_currentIndex];
            bool correct = tappedIndex == current.correctOption;

            if (correct)
            {
                _currentIndex++;
                if (_currentIndex < _items.Count)
                {
                    ShowQuestion(_items[_currentIndex]);
                }
                else
                {
                    questionText.text = "All done!";
                    promptImage.gameObject.SetActive(false);
                    foreach (var b in optionButtons) b.gameObject.SetActive(false);
                }
            }
            else
            {
                // wrong answer: stay on the same question, let them try again
                // (add a "try again" sound/animation here if you want feedback)
            }
        }
    }
}