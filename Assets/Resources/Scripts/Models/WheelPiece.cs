using UnityEngine;
using static Trivia.GameManager;

namespace Trivia
{
    [System.Serializable]
    public class WheelPiece
    {
        public Sprite Icon;
        public string Label;
        public Category Category;

        [Tooltip("Reward amount")]
        public int Amount;

        [Tooltip("Probability in %")]
        [Range(0f, 100f)]
        public float Chance = 100f;

        [HideInInspector] public int Index;
        [HideInInspector] public double _weight = 0f;
    }
}
