using System.IO;
using UnityEngine;
using static Trivia.GameManager;

namespace Trivia
{
    public static class QuestionLoader
    {
        public static void SaveTextQuestion(CategoryModel category)
        {
            string saveFile = GetFilePath(category.Type);
            string json = JsonUtility.ToJson(category.GetTextQuestions());
            File.WriteAllText(saveFile, json);
        }

        public static TextQuestions LoadTextQuestions(Category category)
        {
            string path = GetFilePath(category);
            if (File.Exists(path))
            {
                var jsonString = File.ReadAllText(path);
                var questions = JsonUtility.FromJson<TextQuestions>(jsonString);

                return questions;
            }
            else
                return new TextQuestions();
        }


        static string GetFilePath(Category category) => $"{Application.dataPath}/Resources/Data/Files/Text-{category}.json";
    }
}