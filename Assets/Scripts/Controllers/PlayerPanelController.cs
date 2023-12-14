using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Trivia.GameManager;
using static UnityEngine.UI.CanvasScaler;

namespace Trivia
{
    public class PlayerPanelController : MonoBehaviour
    {
        [SerializeField] Transform template;
        [SerializeField] Transform newPlayerTemplate;
        [SerializeField] bool scoreMode;

        void Start()
        {
            if (!scoreMode)
                StartCoroutine(CreatePlayers());
        }

        public void ShowScore() => StartCoroutine(CreatePlayers());

        public void RemovePlayer(string playerName)
        {
            var model = Instance.m_PlayerConfiguration.m_playersModel;
            model.Players.RemoveAll(p => p.Name == playerName);
            PlayerLoader.SavePlayerConfig(model);
            StartCoroutine(CreatePlayers());
        }

        public void AddPlayer(string playerName, Enums.AvatarType avatar)
        {
            var p = new PlayerModel
            {
                Name = playerName, Avatar = avatar,
                Icon = Resources.Load<Sprite>("Sprites/Avatars/avatar-" + avatar)
            };

            var model = Instance.m_PlayerConfiguration.m_playersModel;
            model.Players.Add(p);
            PlayerLoader.SavePlayerConfig(model);
            StartCoroutine(CreatePlayers());
        }

        IEnumerator CreatePlayers()
        {
            RemovePlayers();

            template.gameObject.SetActive(false);
            newPlayerTemplate.gameObject.SetActive(!scoreMode);

            ShowRemoveButton(template);
            ShowPoints(template);

            while (Instance == null)
                yield return null;

            while (!Instance.m_PlayerConfiguration.m_PlayersLoaded)
                yield return null;

            var players = scoreMode ? Instance.GetPlayers().OrderByDescending(p => p.Points).ToList() : Instance.GetPlayers();
            var position = 1;
            var prevPoints = 0;

            foreach (var player in players)
            {
                var panel = Instantiate(template, template.parent);
                SetName(panel, player.Name);
                SetAvatar(panel, player.Icon);

                if (scoreMode)
                {
                    SetPoints(panel, player.Points);
                    if (player.Points > 0)
                    {
                        if (player.Points < prevPoints)
                            position++;

                        SetScorePosition(panel, position);
                        prevPoints = player.Points;
                    }
                }

                panel.gameObject.SetActive(true);
            }
        }



        void RemovePlayers()
        {
            var container = template.transform.parent;
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                if (container.GetChild(i).gameObject.name.EndsWith("(Clone)"))
                    Destroy(container.GetChild(i).gameObject);
            }
        }

        void ShowRemoveButton(Transform playerTemplate)
        {
            var buttonObj = playerTemplate.GetComponentsInChildren<Button>(true).FirstOrDefault(c => c.name == "Remove Button");
            buttonObj.gameObject.SetActive(!scoreMode);
        }

        void ShowPoints(Transform playerTemplate)
        {
            var pointsObj = playerTemplate.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "points");
            pointsObj.gameObject.SetActive(scoreMode);

        }

        void SetName(Transform panel, string name)
        {
            var nameObj = panel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "name");
            nameObj.text = name;
        }

        void SetAvatar(Transform panel, Sprite image)
        {
            var imageObj = panel.GetComponentsInChildren<Image>(true).FirstOrDefault(c => c.name == "avatar");
            imageObj.sprite = image;
        }

        void SetPoints(Transform panel, int points)
        {
            if (scoreMode || true)
            {
                var pointsObj = panel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "points");

                pointsObj.gameObject.SetActive(true);
                pointsObj.text = $"{points}{(points == 1 ? " punt" : " punten")}";
            }
        }

        void SetScorePosition(Transform panel, int position)
        {
            var first = panel.GetComponentsInChildren<Image>(true).FirstOrDefault(c => c.name == "first");
            var second = panel.GetComponentsInChildren<Image>(true).FirstOrDefault(c => c.name == "second");
            var third = panel.GetComponentsInChildren<Image>(true).FirstOrDefault(c => c.name == "third");

            first.gameObject.SetActive(position == 1);
            second.gameObject.SetActive(position == 2);
            third.gameObject.SetActive(position == 3);
        }
    }
}