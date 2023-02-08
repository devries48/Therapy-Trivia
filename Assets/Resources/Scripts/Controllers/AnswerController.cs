using Trivia;
using UnityEngine;

public class AnswerController : MonoBehaviour
{
    public void OnAnswerClicked()
    {
        print(gameObject.name);
        GameManager.Instance.QuestionAnswered(int.Parse(gameObject.name));
    }
}
