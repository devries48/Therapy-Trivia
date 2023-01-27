using System.Collections;
using TMPro;
using UnityEngine;

namespace Trivia
{
    public class QuestionTextController : MonoBehaviour
    {
        public float delay = 0.1f;
        internal bool Done;

        TextMeshProUGUI _textComponent;

        void Start() => _textComponent = GetComponentInChildren<TextMeshProUGUI>();

        public void SetText(string text) => StartCoroutine(ShowText(text));
        public void ClearText() => _textComponent.text = "";

        IEnumerator ShowText(string text)
        {
            Done = false;
            for (int i = 0; i <= text.Length; i++)
            {
                var currentText = text[..i];
                _textComponent.text = currentText;
                yield return new WaitForSeconds(delay);
            }
            Done = true;
        }

    }
}