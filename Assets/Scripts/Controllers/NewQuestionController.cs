using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Trivia.GameManager;

namespace Trivia
{
    public class NewQuestionController : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown categoryDropdown;
        [SerializeField] TMP_InputField questionText;
        [SerializeField] TMP_InputField completeAnswerText;
        [SerializeField] TMP_InputField answer1Text;
        [SerializeField] TMP_InputField answer2Text;
        [SerializeField] TMP_InputField answer3Text;
        [SerializeField] TMP_InputField answer4Text;
        [SerializeField] Button saveButton;

        [SerializeField] Sprite answerBackground;
        [SerializeField] Sprite correctAnswerBackground;

        int _correctIndex;

        void Start() => StartCoroutine(Initialize());

        public void SetCorrectAnswerClick(int index)
        {
            if (_correctIndex != -1)
                Answer(_correctIndex, false);

            _correctIndex = index;
            Answer(_correctIndex, true);

            CheckInput();
        }

        public void TextInputChanged() => CheckInput();

        void CheckInput()
        {
            int c = 0;

            if (questionText.text.Trim() != "" && _correctIndex != -1)
            {
                if (GetAnswerText(_correctIndex) != "")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (GetAnswerText(i) != "" && c == i)
                            c++;
                    }
                }
            }

            saveButton.interactable = c > 1;
        }

        void Answer(int index, bool isCorrect)
        {
            var img = GetAnswerBackground(index);
            img.sprite = isCorrect ? correctAnswerBackground : answerBackground;
        }

        string GetAnswerText(int index) => index switch
        {
            0 => answer1Text.text.Trim(),
            1 => answer2Text.text.Trim(),
            2 => answer3Text.text.Trim(),
            3 => answer4Text.text.Trim(),
            _ => string.Empty
        };

        Image GetAnswerBackground(int index)
        {
            var text = index switch
            {
                0 => answer1Text,
                1 => answer2Text,
                2 => answer3Text,
                3 => answer4Text,
                _ => null
            };
            return text.gameObject.GetComponentInParent<Image>();
        }

        void SaveQuestion()
        {
            saveButton.interactable = false;

            var categoryName = categoryDropdown.options[categoryDropdown.value].text;
            var category = Instance.GetAllCatagories().First(c => c.Name == categoryName);
            var q = new QuestionTextModel
            {
                Question = questionText.text,
                FullAnswer = completeAnswerText.text,
                Correct = _correctIndex,
                Answers = new()
            };

            for (int i = 0; i < 4; i++)
            {
                var answer = GetAnswerText(i);
                if (answer == "")
                    break;

                q.Answers.Add(answer);
            }

            category.TextQuestions.AddQuestion(q);
            QuestionLoader.SaveTextQuestion(category);
            Instance.QuestionAdded(category.Type);
            ClearInput();
        }

        void ClearInput()
        {
            Answer(_correctIndex, false);
            _correctIndex = -1;

            questionText.text = "";
            completeAnswerText.text = "";
            answer1Text.text = "";
            answer2Text.text = "";
            answer3Text.text = "";
            answer4Text.text = "";
        }

        IEnumerator Initialize()
        {
            ClearInput();

            while (Instance == null)
                yield return null;

            saveButton.onClick.AddListener(() => SaveQuestion());

            var list = Instance.GetAllCatagories().Select(c => c.Name).OrderBy(n => n).ToList();
            if (list.Count > 0)
            {
                categoryDropdown.ClearOptions();
                categoryDropdown.AddOptions(list);
            }
        }

    }
}