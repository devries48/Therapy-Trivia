using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Trivia.GameManager;

namespace Trivia
{
    [CreateAssetMenu(menuName = "Trivia/Create Configuration", fileName = "Trivia Configuration")]
    public class TriviaConfiguration : ScriptableObject
    {
        public List<CategoryModel> Categories;

        public event System.Action<Category> CategoryConfigChangedEvent = delegate { };

        public QuestionTextModel GetTextQuestion(Category category)
        {
            var cat = Categories.FirstOrDefault(c => c.Type == category);
            return cat.TextQuestions.GetQuestion();
        }

        public void RaiseCategoryConfigChangedEvent(Category category) => CategoryConfigChangedEvent(category);
    }


    [System.Serializable]
    public class CategoryModel
    {
        public bool Active;
        public string Name;
        public Category Type;
        public Sprite Icon;
        public TextQuestionConfiguration TextQuestions;

        public int TotalQuestions => TextQuestions == null ? 0 : TextQuestions.TotalQuestions;

        public TextQuestions GetTextQuestions() => TextQuestions.GetTextQuestions();
    }

}