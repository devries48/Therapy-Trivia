using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public enum GameClip
    {
        Ticking,
        Alarm,
        Correct,
        Error
    }

    public enum MusicClip
    {
        startTheme,
        endTheme
    }

    [Header("Game Clips")]
    [SerializeField] AudioClip timerTicking;
    [SerializeField] AudioClip timerAlarm;
    [SerializeField] AudioClip answerCorrect;
    [SerializeField] AudioClip answerError;

    [Header("Music Clips")]
    [SerializeField] AudioClip startTheme;
    [SerializeField] AudioClip endTheme;

    [Header("Audio Sources")]
    [SerializeField] AudioSource gameAudio;
    [SerializeField] AudioSource musicAudio;

    public void PlayMusicClip(MusicClip clip)
    {
        if (clip == MusicClip.startTheme)
        {
            StartCoroutine(FadeIn(musicAudio, .5f));
            PlayMusicAudioClip(startTheme);
        }
        else if (clip == MusicClip.endTheme)
        {
            PlayMusicAudioClip(endTheme);
        }
    }

    public void PlayGameClip(GameClip clip)
    {
        var audioClip = clip switch
        {
            GameClip.Ticking => timerTicking,
            GameClip.Alarm => timerAlarm,
            GameClip.Correct => answerCorrect,
            GameClip.Error => answerError,
            _ => null
        };

        PlayGameAudioClip(audioClip);
    }

    public void StopMusicAudio(bool fade)
    {
        if (fade)
            StartCoroutine(FadeOut(musicAudio, 4f));
        else
            gameAudio.Stop();
    }

    public void StopGameAudio()
    {
        gameAudio.Stop();
    }

    void PlayGameAudioClip(AudioClip clip)
    {
        if (clip && gameAudio)
            gameAudio.PlayOneShot(clip);
    }

    void PlayMusicAudioClip(AudioClip clip)
    {
        if (clip && musicAudio)
            musicAudio.PlayOneShot(clip);
    }

    static bool _isFading;
    static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        while (_isFading)
            yield return null;

        _isFading = true;

        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        _isFading = false;
    }

    static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        while (_isFading)
            yield return null;

        _isFading = true;

        float startVolume = 0.2f;

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = 1f;
        _isFading = false;
    }

}
