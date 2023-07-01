using UnityEngine;
using UnityEngine.Localization.Settings;

public class GameSettings : MonoBehaviour
{
    public static bool OfflineMode;

    [SerializeField] private int _frameRateInScene;
    [SerializeField] private AudioSource _musicSource;

    public enum Scene
    {
        Login,
        Lobby,
        MatchInformer,
        Tutorial,
        Street,
        Roof,
        Fallground
    }

    private void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = _frameRateInScene;

        StringBus stringBus = new();
        int language = PlayerPrefs.GetInt(stringBus.Language);
        SetLanguage(language);

        int musicSettings = PlayerPrefs.GetInt(stringBus.SettingsMusic);
        if (musicSettings == 0 && _musicSource != null) _musicSource.Play();
    }

    private void SetLanguage(int langID)
    {
        //yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.InitializationOperation.WaitForCompletion();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langID];
    }
}
