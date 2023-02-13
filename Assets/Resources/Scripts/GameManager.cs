using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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

        public enum GameStatus { start, menuPanel, spinWheel, question, answered, scorePanel, quit }
        public enum Category { history, nature, science, tv, music, sport }


        [Header("Configuration")]
        public TriviaConfiguration m_TriviaConfiguraton;
        [SerializeField] PlayersConfiguration playerConfiguration;

        [Header("Screens")]
        [SerializeField] GameObject menuCanvas;
        [SerializeField] GameObject gameCanvas;
        [SerializeField] GameObject scoreCanvas;

        [Header("Quiz elements")]
        [SerializeField] QuestionTextController textQuestionController;
        [SerializeField] GameObject answersObject;
        [SerializeField] Button answerTemplate;
        [SerializeField] GameObject countdownTimer;
        [SerializeField] TMPro.TextMeshProUGUI fullAnswerText;

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

        AudioSource _audio;
        GameStatus m_GameStatus;
        CurrentGame _currentGame;
        TimerController _timer;
        AudioSource _timerAudio;
        Image _roundImage;

        int _answerIndex;
        List<Button> _answerButtons;
        private int _correctIndex;

        void Start()
        {
            SingletonInstanceGuard();
            Initialize();
            StartCoroutine(GameLoop());
        }

        public void QuestionAnswered(int index)
        {
            _timer.StopTimer();
            _timerAudio.Stop();
            _answerIndex = index;
            SetGameStatus(GameStatus.answered);
        }

        public void QuestionAdded(Category category) => m_TriviaConfiguraton.RaiseCategoryConfigChangedEvent(category);

        IEnumerator GameLoop()
        {
            SetGameStatus(GameStatus.start);
            _currentGame = new CurrentGame();

            while (m_GameStatus != GameStatus.quit)
            {
                while (m_GameStatus != GameStatus.spinWheel && m_GameStatus != GameStatus.question)
                {
                    if (m_GameStatus == GameStatus.start)
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

        // Wait for the currentplayer to spin the wheel. 
        // - show: full answer
        // - hide: timer, answers
        // - fade out round background
        IEnumerator SpinWheelLoop()
        {
            answersObject.SetActive(false);
            countdownTimer.SetActive(false);
            fullAnswerText.gameObject.SetActive(true);
            spinButton.interactable = true;
            FadeRoundBackground();

            var player = GetCurrentPlayer();
            playerNameText.text = player.Name;
            playerImage.sprite = player.Icon;

            while (m_GameStatus == GameStatus.spinWheel)
                yield return null;
        }

        // Wait for the question to be answered or time runs out.
        // - show: timer, answers
        IEnumerator QuestionStartLoop()
        {
            FadeRoundBackground(true);

            var q = GetTextQuestion();
            textQuestionController.SetText(q.Question);
            fullAnswerText.text = q.FullAnswer != "" ? q.FullAnswer : q.Answers[q.Correct];

            while (!textQuestionController.Done)
                yield return null;

            CreateAnswers(q.Answers, q.Correct);

            countdownTimer.SetActive(true);
            _timer.StartTimer();
            _timerAudio.PlayOneShot(timerTicking);
            _answerIndex = -1;

            while (m_GameStatus == GameStatus.question)
                yield return null;
        }

        // Process the anwers
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

        void Initialize()
        {
            _answerButtons = new List<Button>();
            pickerWheel.SetPieces(GetAllCatagories());

            startButton.onClick.AddListener(() => StartPlaying());
            spinButton.onClick.AddListener(() =>
            {
                spinButton.interactable = false;
                SetButtonText(spinButton, "Draaien");

                pickerWheel.OnSpinStart(() =>
                {
                    fullAnswerText.gameObject.SetActive(false);
                    textQuestionController.ClearText();
                });

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

            _roundImage = answersObject.GetComponentInParent<Image>();

            TryGetComponent(out _audio);

            countdownTimer.TryGetComponent(out _timer);
            countdownTimer.TryGetComponent(out _timerAudio);

            _timer.onTimerEnd.AddListener(HandleTimerEnd);

            foreach (CategoryModel cat in m_TriviaConfiguraton.Categories)
                StartCoroutine(cat.TextQuestions.LoadQuestions());
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

        void FadeRoundBackground(bool fadeIn = false)
        {
            float target;
            Ease easing;

            if (fadeIn)
            {
                target = (float).5f;
                easing = Ease.InQuad;
            }
            else
            {
                target = (float).2f;
                easing = Ease.OutQuad;
            }

            _roundImage.DOFade(target, 1f).SetEase(easing);
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
            m_GameStatus = status;
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

        public List<CategoryModel> GetAllCatagories() => m_TriviaConfiguraton.Categories;

        public List<PlayerModel> GetPlayers() => playerConfiguration.Players;

        PlayerModel GetCurrentPlayer() => playerConfiguration.Players[_currentGame.Player];

        QuestionTextModel GetTextQuestion() => m_TriviaConfiguraton.GetTextQuestion(_currentGame.Category);

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