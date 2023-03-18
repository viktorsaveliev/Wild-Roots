using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Character _character;

    [SerializeField] private Text _level;
    [SerializeField] private Text _levelExp;
    [SerializeField] private Image _levelExpProgressBar;

    /*[SerializeField] private Text _battlePass;
    [SerializeField] private Text _battlePassExp;
    [SerializeField] private Image _battlePassExpProgressBar;*/

    [SerializeField] private Text _winsCount;

    [SerializeField] private GameObject _lobby;
    [SerializeField] private GameObject _registrationPanel;
    public static bool IsGameLoaded = false;

    private void OnEnable()
    {
        EventBus.OnPlayerGetUserIDFromDB += ShowLobby;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerGetUserIDFromDB -= ShowLobby;
    }

    private void Start()
    {
        if(IsGameLoaded)
        {
            _registrationPanel.SetActive(false);
            
            _lobby.SetActive(true);
            Invoke(nameof(UpdateInfo), 1f);
        }
        else
        {
            IsGameLoaded = true;
        }

        LoadingUI.Hide();
    }

    private void UpdateInfo()
    {
        //StringBus stringBus = new();
        //if (PlayerPrefs.GetInt(stringBus.GuestAcc) == 1) return;

        if (_character.Level == 0) _character.Level = 1;

        int maxLevelExp = _character.Level * 3 * 100;
        _level.text = $"{_character.Level}";
        _levelExp.text = $"{_character.Exp}/{maxLevelExp}";

        float exp = _character.Exp + 0.01f;
        _levelExpProgressBar.fillAmount = exp / maxLevelExp;

        _winsCount.text = $"{_character.Wins}";
    }

    private void ShowLobby()
    {
        _lobby.SetActive(true);
        Invoke(nameof(UpdateInfo), 1f);
    }
}
