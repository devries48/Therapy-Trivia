using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Trivia.GameManager;

public class SettingsController : MonoBehaviour
{
    [SerializeField] Toggle optionTimer;
    [SerializeField] Toggle optionRounds;
    [SerializeField] Toggle optionDecrease;
    [SerializeField] Toggle optionDisplayPoints;
    [SerializeField] TMP_Dropdown timerDropdown;
    [SerializeField] TMP_Text roundsText;
    [SerializeField] TMP_Dropdown timerQuestionDropdown;
    [SerializeField] TMP_Dropdown musicDropdown;

    bool _isInitialized = false;
    void Start() => StartCoroutine(Initialize());

    public void IncreaseRounds()
    {
        var val = int.Parse(roundsText.text);
        val++;
        roundsText.text = val.ToString();
    }

    public void DecreaseRounds()
    {
        var val = int.Parse(roundsText.text);
        if (val > 0)
        {
            val--;
            roundsText.text = val.ToString();
        }
    }

    public void SettingsValueChanged()
    {
        if (_isInitialized)
            StartCoroutine(SettingsChanged());
    }
    IEnumerator Initialize()
    {
        while (Instance == null)
            yield return null;

        optionTimer.isOn = Instance.m_PlayerConfiguration.UseGameMinutes;
        optionRounds.isOn = !Instance.m_PlayerConfiguration.UseGameMinutes;
        optionDecrease.isOn = Instance.m_PlayerConfiguration.DecreaseQuestionTime;
        roundsText.text = Instance.m_PlayerConfiguration.TotalGameRounds.ToString();
        optionDisplayPoints.isOn = Instance.m_PlayerConfiguration.DisplayPoints;

        timerDropdown.value = Instance.m_PlayerConfiguration.TotalGameMinutes switch
        {
            5 => 1,
            10 => 2,
            15 => 3,
            20 => 4,
            30 => 5,
            _ => 0
        };

        timerQuestionDropdown.value = Instance.m_PlayerConfiguration.MaxQuestionTime switch
        {
            15 => 1,
            20 => 2,
            25 => 3,
            _ => 0
        };

        musicDropdown.value = Instance.m_PlayerConfiguration.PlayIntroMusic switch
        {
            Trivia.IntroMusic.Round => 1,
            Trivia.IntroMusic.Player => 2,
            _ => 0
        };
        _isInitialized = true;
    }

    IEnumerator SettingsChanged()
    {
        while (Instance == null)
            yield return null;

        Instance.m_PlayerConfiguration.UseGameMinutes = optionTimer.isOn;
        Instance.m_PlayerConfiguration.DecreaseQuestionTime = optionDecrease.isOn;
        Instance.m_PlayerConfiguration.TotalGameRounds = int.Parse(roundsText.text);
        Instance.m_PlayerConfiguration.DisplayPoints = optionDisplayPoints.isOn;

        var minutes = timerDropdown.value switch
        {
            1 => 5,
            2 => 10,
            3 => 15,
            4 => 20,
            5 => 30,
            _ => 1
        };
        Instance.m_PlayerConfiguration.TotalGameMinutes = minutes;

        var seconds = timerQuestionDropdown.value switch
        {
            1 => 15,
            2 => 20,
            3 => 25,
            _ => 10
        };
        Instance.m_PlayerConfiguration.MaxQuestionTime = seconds;

        var music = musicDropdown.value switch
        {
            1 => Trivia.IntroMusic.Round,
            2 => Trivia.IntroMusic.Player,
            _ => Trivia.IntroMusic.Game
        };
        Instance.m_PlayerConfiguration.PlayIntroMusic = music;
    }
}