using UnityEngine;
using UnityEngine.UI;

public class GraphicSetting : MonoBehaviour
{
    [SerializeField] private Image[] _settingButtons;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _inactiveSprite;

    private int _currentQuality;

    private void Awake()
    {
        _currentQuality = PlayerPrefs.GetInt("settings:quality");
        SetSettings(_currentQuality);
        _settingButtons[_currentQuality].sprite = _selectedSprite;
    }

    public void ChangeQuality(int index)
    {
        _settingButtons[_currentQuality].sprite = _inactiveSprite;
        _currentQuality = index;
        SetSettings(index);
        _settingButtons[index].sprite = _selectedSprite;

        SaveGraphicSetting();
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    private void SetSettings(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    private void SaveGraphicSetting()
    {
        PlayerPrefs.SetInt("settings:quality", _currentQuality);
        PlayerPrefs.Save();
    }
}
