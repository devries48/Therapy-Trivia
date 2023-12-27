using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Trivia
{
    [CreateAssetMenu(menuName = "Trivia/Create Players Configuration", fileName = "Trivia Players Configuration")]
    public class PlayersConfiguration : ScriptableObject
    {
        internal bool m_PlayersLoaded;
        internal PlayersModel m_playersModel;

        public bool UseGameMinutes;
        public int TotalGameMinutes;
        public int TotalGameRounds;

        public int MinQuestionTime;
        public int MaxQuestionTime;
        public bool DecreaseQuestionTime;

        public bool DisplayPoints;
        public IntroMusic PlayIntroMusic;

        public List<PlayerModel> Players => m_playersModel.Players;
        public int TotalPlayers => m_playersModel.TotalPlayers;

        public IEnumerator LoadPlayers()
        {
            m_playersModel = PlayerLoader.LoadPlayerConfig();
            if (m_playersModel.Players != null)
            {
                foreach (var player in m_playersModel.Players)
                {
                    player.Icon = Resources.Load<Sprite>("Sprites/Avatars/avatar-" + player.Avatar);
                }
            }

            yield return null;
            m_PlayersLoaded = true;
        }

        public void AddPlayer(PlayerModel player)
        {
            m_playersModel.Players.Add(player);

        }
    }

    public enum IntroMusic
    {
        Game,
        Round,
        Player
    }

    [System.Serializable]
    public class PlayersModel
    {
        public List<PlayerModel> Players;

        public int TotalPlayers => Players?.Count ?? 0;


    }

    [System.Serializable]
    public class PlayerModel
    {
        public string Name;
        public Enums.AvatarType Avatar;

        internal Sprite Icon;
        internal int Points;
    }
}