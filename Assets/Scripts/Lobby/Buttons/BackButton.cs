using UnityEngine;

public class BackButton : MonoBehaviour
{
    [SerializeField] private GameObject _currentScreen;
    [SerializeField] private GameObject _lobby;
    [SerializeField] private GameObject _character;

    [SerializeField] private bool _saveSkinID;

    public void OnClick()
    {
        _character.SetActive(false);
        _lobby.SetActive(true);
        _currentScreen.SetActive(false);

        StringBus stringBus = new();
        if(_saveSkinID && PlayerPrefs.GetInt(stringBus.IsGuest) == 0)
        {
            SaveData.Instance.Skin();
        }
        EventBus.OnPlayerClickUI?.Invoke(0);
        _character.SetActive(true);
    }
}
