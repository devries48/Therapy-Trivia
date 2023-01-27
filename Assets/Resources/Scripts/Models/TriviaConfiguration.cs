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

        internal QuestionTextModel GetTextQuestion(Category category)
        {
            var cat = Categories.FirstOrDefault(c => c.Type == category);
            return cat.TextQuestions.GetQuestion();
        }
    }

    [System.Serializable]
    public class CategoryModel
    {
        public string Name;
        public Category Type;
        public Sprite Icon;
        public TextQuestionConfiguration TextQuestions;
    }

}