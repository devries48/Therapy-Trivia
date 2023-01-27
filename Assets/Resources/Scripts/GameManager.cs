using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using static Trivia.GameManager;

namespace Trivia
{
    public class GameManager : MonoBehaviour
    {
        #region singleton
        public static GameManager Instance => __instance;

        static GameManager __instance;

        void SingletonInstanceGuard()
        {
            if (__instance == null)
            {
                __instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                Destroy(gameObject);
                throw new System.Exception("Only one instance is allowed");
            }
        }
        #endregion

        [Header("Configuration")]
        [SerializeField] TriviaConfiguration triviaConfiguraton;
        [SerializeField] PlayersConfiguration playerConfiguration;

        [Header("Screens")]
        [SerializeField] GameObject menuCanvas;
        [SerializeField] GameObject gameCanvas;
        [SerializeField] GameObject scoreCanvas;

        [Header("Quiz elements")]
        [SerializeField] QuestionTextController textQuestion;
        [SerializeField] GameObject answersObject;
        [SerializeField] Button answerTemplate;
        [SerializeField] GameObject countdownTimer;

        [Header("Answer buton styles")]
        [SerializeField] Sprite normal;
        [SerializeField] Sprite correct;
        [SerializeField] Sprite incorrect;

        [Header("UI elements")]
        [SerializeField] Button startButton;
        [SerializeField] Button stopButton;
        [SerializeField] Button spinButton;
        [SerializeField] PickerWheel pickerWheel;
        [SerializeField] TMPro.TextMeshProUGUI playerNameText;
        [SerializeField] Image playerImage;

        [Header("Audio")]
        [SerializeField] AudioClip timerTicking;
        [SerializeField] AudioClip timerAlarm;
        [SerializeField] AudioClip answerCorrect;
        [SerializeField] AudioClip answerError;

        enum GameStatus { start, menuPanel, spinWheel, question, answered, scorePanel, quit }
        public enum Category { history, nature, science, tv, music }

        AudioSource _audio;
        GameStatus _gameStatus;
        CurrentGame _currentGame;
        TimerController _timer;
        AudioSource _timerAudio;

        int _answerIndex;
        List<Button> _answerButtons;
        private int _correctIndex;

        void Start()
        {
            SingletonInstanceGuard();
            Init();
            StartCoroutine(GameLoop());
        }

        public void QuestionAnswered(int index)
        {
            _timer.StopTimer();
            _timerAudio.Stop();
            _answerIndex = index;
            SetGameStatus(GameStatus.answered);
        }

        IEnumerator GameLoop()
        {
            SetGameStatus(GameStatus.start);
            _currentGame = new CurrentGame();

            while (_gameStatus != GameStatus.quit)
            {
                while (_gameStatus != GameStatus.spinWheel && _gameStatus != GameStatus.question)
                {
                    if (_gameStatus == GameStatus.start)
                    {
                        SetGameStatus(GameStatus.menuPanel);
                        _currentGame.Reset();
                        ShowMenu();
                    }
                    yield return null;
                }

                yield return StartCoroutine(SpinWheelLoop());
                yield return StartCoroutine(QuestionStartLoop());
                yield return StartCoroutine(QuestionAnswerLoop());

                System.GC.Collect();
            }
        }

        void ShowMenu()
        {
            gameCanvas.SetActive(false);
            scoreCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        }

        void ShowScore()
        {
            gameCanvas.SetActive(false);
            menuCanvas.SetActive(false);
            scoreCanvas.SetActive(true);
        }

        void StartPlaying()
        {
            _timer.seconds = playerConfiguration.MaxQuestionTime;

            menuCanvas.SetActive(false);
            scoreCanvas.SetActive(false);
            gameCanvas.SetActive(true);

            SetGameStatus(GameStatus.spinWheel);
        }

        IEnumerator SpinWheelLoop()
        {
            answersObject.SetActive(false);
            countdownTimer.SetActive(false);
            textQuestion.ClearText();
            spinButton.interactable = true;

            var player = GetCurrentPlayer();
            playerNameText.text = player.Name;
            playerImage.sprite = player.Icon;

            while (_gameStatus == GameStatus.spinWheel)
                yield return null;
        }

        IEnumerator QuestionStartLoop()
        {
            var q = GetTextQuestion();
            textQuestion.SetText(q.Question);
            while (!textQuestion.Done)
                yield return null;

            CreateAnswers(q.Answers, q.CorrectAnswerIndex);

            countdownTimer.SetActive(true);
            _timer.StartTimer();
            _timerAudio.PlayOneShot(timerTicking);
            _answerIndex = -1;

            while (_gameStatus == GameStatus.question)
                yield return null;
        }

        IEnumerator QuestionAnswerLoop()
        {
            _currentGame.Question++;

            var buttons = answersObject.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
                if (button.gameObject.name != answerTemplate.gameObject.name)
                    button.interactable = false;

            if (_answerIndex != -1)
                _answerButtons[_answerIndex].image.sprite = _answerIndex == _correctIndex ? correct : incorrect;

            if (_answerIndex != _correctIndex)
            {
                _answerButtons[_correctIndex].image.sprite = correct;
                if (_answerIndex != -1)
                    _audio.PlayOneShot(answerError);

                yield return new WaitForSeconds(3f);

                _currentGame.Player++;
                _currentGame.Round++;

                if (_currentGame.Player > playerConfiguration.TotalPlayers - 1)
                {
                    _currentGame.Player++;
                    if (_currentGame.Round > playerConfiguration.TotalRounds)
                    {
                        SetGameStatus(GameStatus.scorePanel);
                        ShowScore();
                        yield break;
                    }
                }
                SetGameStatus(GameStatus.spinWheel);
            }
            else
            {
                _audio.PlayOneShot(answerCorrect);
                _currentGame.Correct++;

                yield return new WaitForSeconds(2f);

                SetGameStatus(GameStatus.spinWheel);
            }
        }

        void CreateAnswers(List<string> answers, int correctAnswerIndex)
        {
            _correctIndex = correctAnswerIndex;
            _answerButtons.Clear();

            while (answersObject.transform.childCount > 1)
                foreach (Transform child in answersObject.transform)
                    if (child.gameObject.name != answerTemplate.gameObject.name)
                        DestroyImmediate(child.gameObject);

            answerTemplate.gameObject.SetActive(false);
            answersObject.SetActive(true);

            for (int i = 0; i < answers.Count; i++)
            {
                var button = Instantiate(answerTemplate, answersObject.transform);
                SetButtonText(button, answers[i]);

                button.name = i.ToString();
                button.gameObject.SetActive(true);

                _answerButtons.Add(button);
            }
        }


        void Init()
        {
            _answerButtons = new List<Button>();
            pickerWheel.SetPieces(GetCatagories());

            startButton.onClick.AddListener(() => StartPlaying());
            spinButton.onClick.AddListener(() =>
            {
                print("kilk");
                spinButton.interactable = false;
                SetButtonText(spinButton, "Draaien");

                pickerWheel.OnSpinEnd(wheelPiece =>
                {
                    SetCategory(wheelPiece.Category);
                    SetButtonText(spinButton, "Draai");
                });
                pickerWheel.Spin();
            });
            stopButton.onClick.AddListener(() =>
            {
                print("stoppie");
            });

            TryGetComponent(out _audio);

            countdownTimer.TryGetComponent(out _timer);
            countdownTimer.TryGetComponent(out _timerAudio);

            _timer.onTimerEnd.AddListener(HandleTimerEnd);
        }

        void HandleTimerEnd()
        {
            SetGameStatus(GameStatus.answered);

            _timerAudio.Stop();
            _timerAudio.PlayOneShot(timerAlarm);
        }

        void SetGameStatus(GameStatus status)
        {
            print(status);
            _gameStatus = status;
        }
        void SetCategory(Category category)
        {
            _currentGame.Category = category;
            SetGameStatus(GameStatus.question);
        }
        void SetButtonText(Button button, string text)
        {
            var buttontext = button.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            buttontext.text = text;
        }

        List<CategoryModel> GetCatagories()
        {
            return triviaConfiguraton.Categories;
        }

        PlayerModel GetCurrentPlayer() => playerConfiguration.Players[_currentGame.Player];

        QuestionTextModel GetTextQuestion()
        {
            return triviaConfiguraton.GetTextQuestion(_currentGame.Category);
        }

    }

    struct CurrentGame
    {
        public int Player;
        public int Round;
        public int Question;
        public int Correct;

        public Category Category;

        public void Reset()
        {
            Player = 0;
            Round = 1;
        }
    }
}