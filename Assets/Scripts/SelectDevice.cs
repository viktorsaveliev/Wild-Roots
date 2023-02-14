using UnityEngine;

public class SelectDevice : MonoBehaviour
{
    [SerializeField] private GameObject _nextScreen;

    public void Select(int deviceID)
    {
        EventBus.OnPlayerClickUI?.Invoke(0);

        StringBus stringBus = new();
        PlayerPrefs.SetInt(stringBus.PlayerDevice, deviceID);
        PlayerPrefs.Save();
        _nextScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
