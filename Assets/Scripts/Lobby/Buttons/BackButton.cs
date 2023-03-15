using UnityEngine;

public class BackButton : MonoBehaviour
{
    [SerializeField] private GameObject _currentScreen;
    [SerializeField] private GameObject _lobby;
    [SerializeField] private GameObject _character;

    public void OnClick()
    {
        _character.SetActive(true);
        _lobby.SetActive(true);
        _currentScreen.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(0);
    }
}
