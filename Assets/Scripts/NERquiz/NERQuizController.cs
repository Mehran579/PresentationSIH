// Assets/Games/NERMemoryQuiz/NERQuizController.cs
//
// Simple version: show question + image + option buttons.
// Tap the right answer -> next question.
// Tap the wrong answer -> stay on this question, try again.
// Quiz content smoothly scales in when a new question appears.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SmritiCare.Games.NERMemoryQuiz
{
    [System.Serializable]
    public class QuizItem
    {
        public string question;
        public string[] options;
        public int correctOption;
        public string imageUrl;
    }

    [System.Serializable]
    public class QuizContent
    {
        public QuizItem[] items;
    }

    public class NERQuizController : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TextAsset contentJson;

        [Header("UI")]
        [SerializeField] private Transform quizContentRoot;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private Image promptImage;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TMP_Text[] optionLabels;

        [Header("Animation")]
        [SerializeField] private float transitionTime = 0.25f;

        private List<QuizItem> _items;
        private int _currentIndex;
        private bool _isTransitioning;

        private void Start()
        {
            QuizContent parsed = JsonUtility.FromJson<QuizContent>(contentJson.text);

            _items = new List<QuizItem>(parsed.items);
            _currentIndex = 0;

            ShowQuestion(_items[_currentIndex]);
        }

        private void ShowQuestion(QuizItem item)
        {
            StartCoroutine(ShowQuestionAnimated(item));
        }

        private IEnumerator ShowQuestionAnimated(QuizItem item)
        {
            _isTransitioning = true;

            // Set the new question first
            questionText.text = item.question;

            // Set image
            if (!string.IsNullOrEmpty(item.imageUrl))
            {
                promptImage.sprite =
                    Resources.Load<Sprite>($"Images/{item.imageUrl}");

                promptImage.gameObject.SetActive(true);
            }
            else
            {
                promptImage.gameObject.SetActive(false);
            }

            // Set options
            for (int i = 0; i < optionButtons.Length; i++)
            {
                bool hasOption = i < item.options.Length;

                optionButtons[i].gameObject.SetActive(hasOption);

                if (!hasOption)
                    continue;

                optionLabels[i].text = item.options[i];

                int optionIndex = i;

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(
                    () => OnAnswerTapped(optionIndex)
                );
            }

            // Start slightly smaller.
            // Do NOT use zero — this keeps the UI visible even if
            // something interrupts the animation.
            quizContentRoot.localScale = Vector3.one * 0.8f;

            float elapsed = 0f;

            while (elapsed < transitionTime)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / transitionTime);

                // Smooth ease-out
                t = 1f - Mathf.Pow(1f - t, 3f);

                quizContentRoot.localScale = Vector3.Lerp(
                    Vector3.one * 0.8f,
                    Vector3.one,
                    t
                );

                yield return null;
            }

            // Guarantee the final state.
            quizContentRoot.localScale = Vector3.one;

            _isTransitioning = false;
        }

        private void OnAnswerTapped(int tappedIndex)
        {
            if (_isTransitioning)
                return;

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
                    FinishQuiz();
                }
            }
            else
            {
                // Wrong answer:
                // Stay on the same question.
                // No shake, no punishment.
            }
        }

        private void FinishQuiz()
        {
            questionText.text = "All done!";
            promptImage.gameObject.SetActive(false);

            foreach (var b in optionButtons)
                b.gameObject.SetActive(false);
        }
    }
}