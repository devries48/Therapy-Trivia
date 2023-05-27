using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Trivia
{
    public class PickerWheel : MonoBehaviour
    {
        [Header("References :")]
        [SerializeField] GameObject linePrefab;
        [SerializeField] Transform linesParent;

        [Space]
        [SerializeField] Transform PickerWheelTransform;
        [SerializeField] Transform wheelCircle;
        [SerializeField] GameObject wheelPiecePrefab;
        [SerializeField] Transform wheelPiecesParent;

        [Space]
        [Header("Sounds :")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip tickAudioClip;
        [SerializeField][Range(0f, 1f)] float volume = .5f;
        [SerializeField][Range(-3f, 3f)] float pitch = 1f;

        [Space]
        [Header("Picker wheel settings :")]
        [Range(1, 20)] public int spinDuration = 8;
        [SerializeField][Range(.2f, 2f)] float wheelSize = 1f;

        [Space]
        [Header("Picker wheel pieces :")]
        public WheelPiece[] wheelPieces;

        // Events
        UnityAction onSpinStartEvent;
        UnityAction<WheelPiece> onSpinEndEvent;

        bool _isSpinning = false;

        public bool IsSpinning { get { return _isSpinning; } }

        Vector2 pieceMinSize = new(81f, 146f);
        Vector2 pieceMaxSize = new(144f, 213f);
        readonly int piecesMin = 2;
        readonly int piecesMax = 12;

        float pieceAngle;
        float halfPieceAngle;
        float halfPieceAngleWithPaddings;

        double accumulatedWeight;
        readonly System.Random rand = new();
        readonly List<int> nonZeroChancesIndices = new();

        void Start()
        {
            SetupPieces();
            SetupAudio();
        }


        public void SetPieces(List<CategoryModel> categories)
        {
            var pieces = new List<WheelPiece>();

            foreach (var category in categories)
            {
                var piece = new WheelPiece
                {
                    Icon = category.Icon,
                    Label = category.Name,
                    Category = category.Type,
                    Chance = 100
                };
                pieces.Add(piece);
            }

            wheelPieces = pieces.ToArray();
            SetupPieces();
            Generate();

            CalculateWeightsAndIndices();

            if (nonZeroChancesIndices.Count == 0)
                Debug.LogError("You can't set all pieces chance to zero");
        }

        void SetupPieces()
        {
            if (wheelPieces.Length == 0) return;

            pieceAngle = 360 / wheelPieces.Length;
            halfPieceAngle = pieceAngle / 2f;
            halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle / 4f);
        }

        void SetupAudio()
        {
            audioSource.clip = tickAudioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
        }

        void Generate()
        {
            wheelPiecePrefab = InstantiatePiece();

            RectTransform rt = wheelPiecePrefab.transform.GetChild(0).GetComponent<RectTransform>();
            float pieceWidth = Mathf.Lerp(pieceMinSize.x, pieceMaxSize.x, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            float pieceHeight = Mathf.Lerp(pieceMinSize.y, pieceMaxSize.y, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);

            for (int i = 0; i < wheelPieces.Length; i++)
                DrawPiece(i);

            Destroy(wheelPiecePrefab);
        }

        void DrawPiece(int index)
        {
            WheelPiece piece = wheelPieces[index];
            Transform pieceTrns = InstantiatePiece().transform.GetChild(0);

            pieceTrns.GetChild(0).GetComponent<Text>().text = piece.Label;
            pieceTrns.GetChild(1).GetComponent<Image>().sprite = piece.Icon;
            pieceTrns.GetChild(2).GetComponent<Text>().text = piece.Amount.ToString();

            //Line
            Transform lineTrns = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent).transform;
            lineTrns.RotateAround(wheelPiecesParent.position, Vector3.back, (pieceAngle * index) + halfPieceAngle);

            pieceTrns.RotateAround(wheelPiecesParent.position, Vector3.back, pieceAngle * index);
        }

        GameObject InstantiatePiece()
        {
            return Instantiate(wheelPiecePrefab, wheelPiecesParent.position, Quaternion.identity, wheelPiecesParent);
        }

        public void Spin()
        {
            if (!_isSpinning)
            {
                _isSpinning = true;
                onSpinStartEvent?.Invoke();

                int index = GetRandomPieceIndex();
                WheelPiece piece = wheelPieces[index];

                if (piece.Chance == 0 && nonZeroChancesIndices.Count != 0)
                {
                    index = nonZeroChancesIndices[Random.Range(0, nonZeroChancesIndices.Count)];
                    piece = wheelPieces[index];
                }

                float angle = -(pieceAngle * index);
                float rightOffset = (angle - halfPieceAngleWithPaddings) % 360;
                float leftOffset = (angle + halfPieceAngleWithPaddings) % 360;
                float randomAngle = Random.Range(leftOffset, rightOffset);
                
                Vector3 targetRotation = Vector3.back * (randomAngle + 2 * 360 * spinDuration);

                //float prevAngle = wheelCircle.eulerAngles.z + halfPieceAngle ;
                bool isIndicatorOnTheLine = false;
                float prevAngle, currentAngle;
                prevAngle = currentAngle = wheelCircle.eulerAngles.z;

                wheelCircle
                    .DORotate(targetRotation, spinDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.InOutQuart)
                    .OnUpdate(() =>
                    {
                        float diff = Mathf.Abs(prevAngle - currentAngle);
                        if (diff >= halfPieceAngle)
                        {
                            if (isIndicatorOnTheLine)
                            {
                                audioSource.PlayOneShot(audioSource.clip);
                            }
                            prevAngle = currentAngle;
                            isIndicatorOnTheLine = !isIndicatorOnTheLine;
                        }
                        currentAngle = wheelCircle.eulerAngles.z;
                    })
                    .OnComplete(() =>
                    {
                        _isSpinning = false;
                        onSpinEndEvent?.Invoke(piece);

                        onSpinStartEvent = null;
                        onSpinEndEvent = null;
                    });

            }
        }

        public void OnSpinStart(UnityAction action)
        {
            onSpinStartEvent = action;
        }

        public void OnSpinEnd(UnityAction<WheelPiece> action)
        {
            onSpinEndEvent = action;
        }

        int GetRandomPieceIndex()
        {
            double r = rand.NextDouble() * accumulatedWeight;

            for (int i = 0; i < wheelPieces.Length; i++)
                if (wheelPieces[i]._weight >= r)
                    return i;

            return 0;
        }

        void CalculateWeightsAndIndices()
        {
            for (int i = 0; i < wheelPieces.Length; i++)
            {
                WheelPiece piece = wheelPieces[i];

                //add weights:
                accumulatedWeight += piece.Chance;
                piece._weight = accumulatedWeight;

                //add index :
                piece.Index = i;

                //save non zero chance indices:
                if (piece.Chance > 0)
                    nonZeroChancesIndices.Add(i);
            }
        }

        void OnValidate()
        {
            if (PickerWheelTransform != null)
                PickerWheelTransform.localScale = new Vector3(wheelSize, wheelSize, 1f);

            //if (wheelPieces.Length > piecesMax || wheelPieces.Length < piecesMin)
            //    Debug.LogError("[ PickerWheelwheel ]  pieces length must be between " + piecesMin + " and " + piecesMax);
        }
    }
}