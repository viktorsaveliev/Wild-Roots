using UnityEngine;

public class SettingButton : MonoBehaviour
{
    [SerializeField] private GameObject _settings;

    public void OpenSettings()
    {
        _settings.SetActive(true);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    public void CloseSettings()
    {
        _settings.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }
}
