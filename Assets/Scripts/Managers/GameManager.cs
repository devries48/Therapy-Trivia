using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Enums;

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

        #region editor fields
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
        [SerializeField] TextMeshProUGUI fullAnswerText;

        [Header("Answer button styles")]
        [SerializeField] Sprite normal;
        [SerializeField] Sprite correct;
        [SerializeField] Sprite incorrect;

        [Header("Buttons")]
        [SerializeField] Button startButton;
        [SerializeField] Button stopButton;
        [SerializeField] Button spinButton;
        [SerializeField] Button newGameButton;
        [SerializeField] Button quitGameButton;

        [Header("UI elements")]
        [SerializeField] PickerWheel pickerWheel;
        [SerializeField] TextMeshProUGUI[] playerNameText;
        [SerializeField] Image playerImage;
        //[SerializeField] TextMeshProUGUI titleRoundsText;
        [SerializeField] TextMeshProUGUI titleTimerText;

        [Header("Audio")]
        [SerializeField] AudioManager audioManager;
        #endregion

        #region fields
        IUserInputManager _userInput;
        GameStatus _GameStatus;
        CurrentGame _currentGame;
        TimerController _timerQuestion;
        TimerController _timerGame;
        bool _timerGameStarted;
        Image _roundImage;

        int _answerIndex;
        int _correctIndex;
        List<Button> _answerButtons;

        #endregion
        public TextMeshProUGUI[] PlayerNameText { get => playerNameText; set => playerNameText = value; }

        void Awake()
        {
            SingletonInstanceGuard();
            _userInput = FindObjectOfType<UserInputManager>();
        }

        void Start()
        {
            Initialize();
            StartCoroutine(GameLoop());
        }

        void OnEnable()
        {
            _userInput.OnAnswer1Received += HandleAnswer1;
            _userInput.OnAnswer2Received += HandleAnswer2;
            _userInput.OnAnswer3Received += HandleAnswer3;
            _userInput.OnAnswer4Received += HandleAnswer4;
            _userInput.OnSubmitReceived += HandleSubmitted;
            _userInput.OnCancelReceived += HandleCancelled;
        }

        void OnDisable()
        {
            _userInput.OnAnswer1Received -= HandleAnswer1;
            _userInput.OnAnswer2Received -= HandleAnswer2;
            _userInput.OnAnswer3Received -= HandleAnswer3;
            _userInput.OnAnswer4Received -= HandleAnswer4;
            _userInput.OnSubmitReceived -= HandleSubmitted;
            _userInput.OnCancelReceived -= HandleCancelled;
        }

        #region public mehods (GetAllCatagories, GetPlayers, QuestiomAdded, QuestionAnswered)
        public List<CategoryModel> GetAllCatagories() => m_TriviaConfiguraton.Categories;
        public List<PlayerModel> GetPlayers() => playerConfiguration.Players;

        public void QuestionAnswered(int index)
        {
            SetGameStatus(GameStatus.answered);

            _timerQuestion.StopTimer();
            _answerIndex = index;

            audioManager.StopGameAudio();
        }

        public void QuestionAdded(Category category) => m_TriviaConfiguraton.RaiseCategoryConfigChangedEvent(category);

        #endregion

        #region gameflow/loops
        void StartPlaying()
        {
            if (playerConfiguration.useGameMinutes)
            {
                _timerGame.StopTimer();
                _timerGame.minutes = playerConfiguration.TotalGameMinutes;
            }

            titleTimerText.gameObject.SetActive(false);
            audioManager.PlayMusicClip(AudioManager.MusicClip.startTheme);

            menuCanvas.SetActive(false);
            scoreCanvas.SetActive(false);
            gameCanvas.SetActive(true);

            SetGameStatus(GameStatus.spinWheel);
        }

        IEnumerator GameLoop()
        {
            SetGameStatus(GameStatus.start);
            _currentGame = new CurrentGame();

            while (_GameStatus != GameStatus.quit)
            {
                while (_GameStatus != GameStatus.spinWheel && _GameStatus != GameStatus.question)
                {
                    if (_GameStatus == GameStatus.start)
                    {
                        _currentGame.Reset(playerConfiguration);
                        _timerGameStarted = false;

                        ShowMenu();
                    }
                    else if (_GameStatus == GameStatus.stop)
                    {
                        ShowMenu();
                    }
                    else if (_GameStatus == GameStatus.quit)
                        break;

                    yield return null;
                }

                if (_GameStatus == GameStatus.quit)
                    break;

                yield return StartCoroutine(SpinWheelLoop());
                yield return StartCoroutine(QuestionStartLoop());
                yield return StartCoroutine(QuestionAnswerLoop());

                System.GC.Collect();
            }
            GameQuit();
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

            ActivateSpinButton(true);
            FadeRoundBackground();
            SetPlayer();

            while (_GameStatus == GameStatus.spinWheel)
                yield return null;
        }

        // Wait for the question to be answered or time runs out.
        // - show: timer, answers
        IEnumerator QuestionStartLoop()
        {
            if (_GameStatus == GameStatus.stop)
                yield break;

            FadeRoundBackground(true);

            var q = GetTextQuestion();
            textQuestionController.SetText(q.Question);
            fullAnswerText.text = q.FullAnswer != "" ? q.FullAnswer : q.Answers[q.Correct];

            while (!textQuestionController.Done)
                yield return null;

            CreateAnswers(q.Answers, q.Correct);

            countdownTimer.SetActive(true);
            audioManager.PlayGameClip(AudioManager.GameClip.Ticking);
            _timerQuestion.StartTimer();
            _answerIndex = -1;

            while (_GameStatus == GameStatus.question)
                yield return null;
        }

        // Process the anwers
        IEnumerator QuestionAnswerLoop()
        {
            if (_GameStatus == GameStatus.stop)
                yield break;

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
                    audioManager.PlayGameClip(AudioManager.GameClip.Error);

                yield return new WaitForSeconds(3f);

                _currentGame.PlayerNr++;
                _currentGame.Round++;

                if (_currentGame.PlayerNr > playerConfiguration.TotalPlayers - 1)
                {
                    _currentGame.PlayerNr = 0;

                    if (!playerConfiguration.useGameMinutes && _currentGame.Round > playerConfiguration.TotalGameRounds
                        || playerConfiguration.useGameMinutes && _timerGame.GetRemainingSeconds() == 0)
                    {
                        ShowScore();
                        yield break;
                    }
                }
                SetGameStatus(GameStatus.spinWheel);
            }
            else
            {
                audioManager.PlayGameClip(AudioManager.GameClip.Correct);
                _currentGame.AddScore();

                yield return new WaitForSeconds(2f);

                SetGameStatus(GameStatus.spinWheel);
            }
        }

        #endregion

        #region initialize
        void Initialize()
        {
            _answerButtons = new List<Button>();
            pickerWheel.SetPieces(GetAllCatagories());

            startButton.onClick.AddListener(() => StartPlaying());
            stopButton.onClick.AddListener(() => SetGameStatus(GameStatus.stop));
            newGameButton.onClick.AddListener(() => SetGameStatus(GameStatus.start));
            quitGameButton.onClick.AddListener(() => SetGameStatus(GameStatus.quit));
            spinButton.onClick.AddListener(() => StartSpinning());

            _roundImage = answersObject.GetComponentInParent<Image>();
            TryGetComponent(out _timerGame);
            countdownTimer.TryGetComponent(out _timerQuestion);

            _timerQuestion.onTimerEnd.AddListener(HandleTimerEnd);
            _timerGame.onTimerEnd.AddListener(HandleGameTimerEnd);

            foreach (CategoryModel cat in m_TriviaConfiguraton.Categories)
                StartCoroutine(cat.TextQuestions.LoadQuestions());
        }
        #endregion

        #region input

        //public enum GameStatus { start, menuPanel, spinWheel, question, answered, scorePanel, stop, quit }

        void HandleAnswer1() => HandleAnswer(0);
        void HandleAnswer2() => HandleAnswer(1);
        void HandleAnswer3() => HandleAnswer(2);
        void HandleAnswer4() => HandleAnswer(3);

        void HandleAnswer(int index)
        {
            if (_GameStatus == GameStatus.question)
                QuestionAnswered(index);
        }

        void HandleSubmitted()
        {
            if (_GameStatus == GameStatus.menuPanel)
                StartPlaying();
            else if (_GameStatus == GameStatus.spinWheel)
                StartSpinning();
            else if (_GameStatus == GameStatus.scorePanel)
                SetGameStatus(GameStatus.start);
        }

        void HandleCancelled()
        {
            if (_GameStatus == GameStatus.menuPanel)
                SetGameStatus(GameStatus.stop);
        }

        #endregion

        void ShowMenu()
        {
            SetGameStatus(GameStatus.menuPanel);

            audioManager.StopGameAudio();
            audioManager.StopMusicAudio(false);

            gameCanvas.SetActive(false);
            scoreCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        }

        void ShowScore()
        {
            SetGameStatus(GameStatus.scorePanel);

            gameCanvas.SetActive(false);
            menuCanvas.SetActive(false);
            scoreCanvas.SetActive(true);

            var playerPanel = scoreCanvas.GetComponentInChildren<PlayerPanelController>(true);
            playerPanel.ShowScore();

            audioManager.PlayMusicClip(AudioManager.MusicClip.endTheme);
        }

        void CreateAnswers(List<string> answers, int correctAnswerIndex)
        {
            _correctIndex = correctAnswerIndex;
            _answerButtons.Clear();
            UserInputManager.Instance.ClearAnswerButtons();

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
                UserInputManager.Instance.AddAnswerButton(button, i);
                button.name = i.ToString();
                button.gameObject.SetActive(true);

                _answerButtons.Add(button);
            }
        }

        void ActivateSpinButton(bool active)
        {
            spinButton.interactable = active;
            spinButton.transform.DOMoveY(active ? 80 : -80, 1f).SetEase(Ease.InOutSine);

            var toScale = active ? new Vector3(1f, 1f, 1f) : new Vector3(.1f, .1f, .1f);
            PlayerNameText[1].transform.localScale = active ? new Vector3(.1f, .1f, .1f) : new Vector3(1f, 1f, 1f);

            if (active)
                PlayerNameText[1].gameObject.SetActive(true);

            PlayerNameText[1].transform.DOScale(toScale, 1f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    if (!active) PlayerNameText[1].gameObject.SetActive(false);
                });
        }

        void StartSpinning()
        {
            ActivateSpinButton(false);
            SetButtonText(spinButton, "Draaien");

            pickerWheel.OnSpinStart(() =>
            {
                fullAnswerText.gameObject.SetActive(false);
                textQuestionController.ClearText();
                audioManager.StopMusicAudio(true);
            });

            pickerWheel.OnSpinEnd(wheelPiece =>
            {
                SetCategory(wheelPiece.Category);
                SetButtonText(spinButton, "Draai");
            });
            pickerWheel.Spin();
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
            audioManager.StopGameAudio();
            audioManager.PlayGameClip(AudioManager.GameClip.Alarm);
        }

        void HandleGameTimerEnd()
        {
            titleTimerText.text = "Laatste ronde";
            print("Laatste ronde");
        }

        void SetPlayer()
        {
            var player = GetCurrentPlayer();

            if (_currentGame.Player != player)
            {
                _currentGame.Player = player;
                _currentGame.QuestionTime = playerConfiguration.MaxQuestionTime;
                _timerQuestion.seconds = _currentGame.QuestionTime;
            }
            else
            {
                _timerQuestion.seconds = _currentGame.QuestionCorrect(playerConfiguration);
                return;
            }

            PlayerNameText[0].text = player.Name;
            PlayerNameText[1].text = player.Name;
            playerImage.sprite = player.Icon;

            var orgScale = playerImage.transform.localScale;
            var scaleTo = orgScale * 1.5f;

            playerImage.transform.DOScale(scaleTo, .5f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    playerImage.transform.DOScale(orgScale, .5f)
                        .SetEase(Ease.OutBounce)
                        .SetDelay(.25f);
                });
        }

        void SetGameStatus(GameStatus status)
        {
            print($"STATUS: {status}");
            _GameStatus = status;
        }

        void SetCategory(Category category)
        {
            if (playerConfiguration.useGameMinutes)
            {
                if (!_timerGameStarted)
                {
                    _timerGameStarted = true;
                    titleTimerText.gameObject.SetActive(true);
                    _timerGame.StartTimer();
                }
            }

            _currentGame.Category = category;
            SetGameStatus(GameStatus.question);
        }

        void SetButtonText(Button button, string text)
        {
            var buttontext = button.GetComponentInChildren<TextMeshProUGUI>(true);
            buttontext.text = text;
        }

        PlayerModel GetCurrentPlayer() => playerConfiguration.Players[_currentGame.PlayerNr];

        QuestionTextModel GetTextQuestion() => m_TriviaConfiguraton.GetTextQuestion(_currentGame.Category);

        void GameQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }
    }

    struct CurrentGame
    {
        public PlayerModel Player;
        public int PlayerNr;
        public int Round;
        public int Question;
        public Category Category;
        public int QuestionTime;

        public void AddScore() => Player.Points++;

        public int QuestionCorrect(PlayersConfiguration config)
        {
            if (config.DecreaseQuestionTime && QuestionTime > config.MinQuestionTime)
                QuestionTime--;

            return QuestionTime;
        }

        public void Reset(PlayersConfiguration config)
        {
            PlayerNr = 0;
            Round = 1;

            foreach (var player in config.Players)
                player.Points = 0;

        }
    }
}