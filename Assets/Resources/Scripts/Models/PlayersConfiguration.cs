using System.Collections.Generic;
using UnityEngine;

namespace Trivia
{
    [CreateAssetMenu(menuName = "Trivia/Create Players Configuration", fileName = "Trivia Players Configuration")]
    public class PlayersConfiguration : ScriptableObject
    {
        public bool useGameMinutes;
        public int TotalGameMinutes;
        public int TotalGameRounds;

        public int MaxQuestionTime;
        public bool DecreaseQuestionTime;

        public List<PlayerModel> Players;
        public int TotalPlayers => Players?.Count ?? 0;
    }

    [System.Serializable]
    public class PlayerModel
    {
        public string Name;
        public Sprite Icon;

        internal int Rounds;
        internal int Questions;
        internal int Points;
    }

}