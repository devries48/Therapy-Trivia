using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Trivia
{
    [CreateAssetMenu(menuName = "Trivia/Create Text Questions", fileName = "Trivia Text Questions (catergory)")]
    public class TextQuestionConfiguration : ScriptableObject
    {
        public List<QuestionTextModel> Questions;
        public int TotalQuestions => Questions?.Count ?? 0;

        internal QuestionTextModel GetQuestion()
        {
            var questions = Questions.Where(q => !q.isAsked).ToList();

            if (!questions.Any() && TotalQuestions > 0)
            {
                ResetAskedFlags();
                questions = Questions.Where(q => !q.isAsked).ToList();
            }
            else
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

            int index = Random.Range(0, questions.Count());
            return questions[index];
        }

        public void ResetAskedFlags() => Questions.Where(q => q.isAsked).Select(q => { q.isAsked = false; return q; }).ToList();
    }

    [System.Serializable]
    public class QuestionTextModel
    {
        public string Question;
        public List<string> Answers;
        public int CorrectAnswerIndex;

        internal bool isAsked;
    }
}