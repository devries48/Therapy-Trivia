using Trivia;
using UnityEngine;

public class AnswerController : MonoBehaviour
{
    public void OnAnswerClicked()
    {
        GameManager.Instance.QuestionAnswered(int.Parse(gameObject.name));
    }
}
