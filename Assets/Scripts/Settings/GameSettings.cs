using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GameSettings : MonoBehaviour
{
    public static SelectGameMode.GameMode GameMode;

    [SerializeField] private int _frameRateInScene;
    [SerializeField] private AudioSource _musicSource;

    void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = _frameRateInScene;

        StringBus stringBus = new();
        int language = PlayerPrefs.GetInt(stringBus.Language);
        StartCoroutine(SetLanguage(language));

        int musicSettings = PlayerPrefs.GetInt(stringBus.SettingsMusic);
        if (musicSettings == 0 && _musicSource != null) _musicSource.Play();
    }

    private IEnumerator SetLanguage(int langID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langID];
    }

    private void Start()
    {
        Notice.HideDialog();
    }
}
