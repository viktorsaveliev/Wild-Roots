using UnityEngine;

public class WinnerShower : MonoBehaviour
{
    [SerializeField] private GameObject _defeatScreen;
    [SerializeField] private GameObject _winnerScreen;
    [SerializeField] private GameObject _charOnScreen;
    [SerializeField] private GameObject _interface;
    [SerializeField] private GameObject _buttonMenu;

    private void OnEnable()
    {
        EventBus.OnPlayerWin += ShowWinnerScreen;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerWin -= ShowWinnerScreen;
    }

    private void ShowWinnerScreen()
    {
        _winnerScreen.SetActive(true);
        _charOnScreen.SetActive(true);
        _buttonMenu.SetActive(true);

        _interface.SetActive(false);
        _defeatScreen.SetActive(false);
    }
}
