using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Trivia.GameManager;

namespace Trivia
{
    [CreateAssetMenu(menuName = "Trivia/Create Text Questions", fileName = "Trivia Text Questions (catergory)")]
    public class TextQuestionConfiguration : ScriptableObject
    {
        public Category category;

        public int TotalQuestions => Questions.List?.Count ?? 0;

        TextQuestions Questions;
        internal bool m_QuestionsLoaded;

        public IEnumerator LoadQuestions()
        {
            Questions = QuestionLoader.LoadTextQuestions(category);
            yield return null;
            m_QuestionsLoaded = true;
        }

        public void AddQuestion(QuestionTextModel question)
        {
            Questions.List.Add(question);
        }

        public TextQuestions GetTextQuestions() => Questions;

        public QuestionTextModel GetQuestion()
        {
            if (TotalQuestions == 0)
            {
                return new QuestionTextModel
                {
                    Question = "Nog geen vragen aanwezig voor deze categorie.",
                    Answers = new List<string>
                    {
                        "Dit is het goede antwoord!",
                        "Dit niet helaas..."
                    }
                };
            }

            var questions = Questions.List?.Where(q => !q.isAsked).ToList();

            if (!questions.Any())
            {
                Debug.Log("ResetAskedFlags");

                ResetAskedFlags();
                questions = Questions.List?.Where(q => !q.isAsked).ToList();
            }

            int index = Random.Range(0, questions.Count());
            var q = questions[index];
            q.isAsked= true;

            return q;
        }

        public void ResetAskedFlags() => Questions.List?.Where(q => q.isAsked).Select(q => { q.isAsked = false; return q; }).ToList();
    }

    [System.Serializable]
    public class QuestionTextModel
    {
        public string Question;
        public List<string> Answers;
        public int Correct;
        public string FullAnswer;

        internal bool isAsked;
    }

    [System.Serializable]
    public class TextQuestions
    {
        public TextQuestions() => List = new();
        public List<QuestionTextModel> List;
    }

}