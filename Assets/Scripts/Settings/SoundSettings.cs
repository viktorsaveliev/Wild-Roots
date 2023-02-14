using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    [SerializeField] private Button _typeButton;
    [SerializeField] private Sprite[] _buttonSprite;
    [SerializeField] private AudioSource _musicLobby;
    private Image _image;

    private enum Button
    {
        SoundFX,
        Music
    }

    private void Start()
    {
        _image = GetComponent<Image>();
        SetSetting();
    }

    public void ChangeSoundSettings()
    {
        StringBus stringBus = new();
        int currentSoundSettings;

        if (_typeButton == Button.SoundFX)
        {
            currentSoundSettings = PlayerPrefs.GetInt(stringBus.SettingsSoundFX);
            PlayerPrefs.SetInt(stringBus.SettingsSoundFX, currentSoundSettings == 1 ? 0 : 1);
        }
        else
        {
            currentSoundSettings = PlayerPrefs.GetInt(stringBus.SettingsMusic);
            PlayerPrefs.SetInt(stringBus.SettingsMusic, currentSoundSettings == 1 ? 0 : 1);
            if (currentSoundSettings == 1)
            {
                _musicLobby.Play();
            }
            else
            {
                _musicLobby.Stop();
            }
        }

        _image.sprite = _buttonSprite[currentSoundSettings == 1 ? 0 : 1];
        PlayerPrefs.Save();
        EventBus.OnPlayerClickUI?.Invoke(1);
    }    

    private void SetSetting()
    {
        StringBus stringBus = new();
        int currentSoundSettings;

        if (_typeButton == Button.SoundFX)
        {
            currentSoundSettings = PlayerPrefs.GetInt(stringBus.SettingsSoundFX);
        }
        else
        {
            currentSoundSettings = PlayerPrefs.GetInt(stringBus.SettingsMusic);
            
        }
        _image.sprite = _buttonSprite[currentSoundSettings];
    }
}
