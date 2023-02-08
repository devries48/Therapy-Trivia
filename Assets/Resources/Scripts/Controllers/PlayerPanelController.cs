using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Trivia.GameManager;

namespace Trivia
{
    public class PlayerPanelController : MonoBehaviour
    {
        [SerializeField] Transform _template;
        [SerializeField] bool _scoreMode;

        void Start() => StartCoroutine(Initialize());

        IEnumerator Initialize()
        {
            _template.gameObject.SetActive(false);

            while (Instance == null)
                yield return null;

            foreach (var player in Instance.GetPlayers())
            {
                var panel = Instantiate(_template, transform);
                SetName(panel, player.Name);
                SetAvatar(panel, player.Icon);
                SetPoints(panel, player.Points);

                panel.gameObject.SetActive(true);
            }
        }

        void SetName(Transform panel, string name)
        {
            var nameObj = panel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "name");

            if (nameObj != null)
                nameObj.text = name;
        }

        void SetAvatar(Transform panel, Sprite image)
        {
            var imageObj = panel.GetComponentsInChildren<Image>(true).FirstOrDefault(c => c.name == "avatar");

            if (imageObj != null)
                imageObj.sprite = image;
        }

        void SetPoints(Transform panel, int points)
        {
            if (_scoreMode)
            {
                var pointsObj = panel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "points");

                if (pointsObj != null)
                {
                    pointsObj.gameObject.SetActive(true);
                    pointsObj.text = points.ToString();
                }
            }
        }

    }
}