using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageButton : MonoBehaviour
{
    private bool _isActive;
    public void ChangeLanguage()
    {
        if (_isActive) return;

        TextLanguagesData languageData = new();

        StringBus stringBus = new();
        int currentLanguage = PlayerPrefs.GetInt(stringBus.Language);

        if (++currentLanguage >= languageData.MAX_LANGUAGE)
        {
            currentLanguage = 0;
        }

        StartCoroutine(SetLanguage(currentLanguage));
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    private IEnumerator SetLanguage(int langID)
    {
        _isActive = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langID];

        StringBus stringBus = new();
        PlayerPrefs.SetInt(stringBus.Language, langID);
        PlayerPrefs.Save();

        _isActive = false;
    }
}
