using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Character _character;

    [SerializeField] private Text _level;
    [SerializeField] private Text _levelExp;
    [SerializeField] private Image _levelExpProgressBar;

    [SerializeField] private Text _coinsText;
    [SerializeField] private Text _differenceText;
    [SerializeField] private Animator _animator;

    [SerializeField] private Text _winsCount;

    [SerializeField] private GameObject _lobby;
    [SerializeField] private GameObject _registrationPanel;

    private int _coins = 0;

    private void OnEnable()
    {
        EventBus.OnPlayerGetUserIDFromDB += ShowLobby;
        EventBus.OnPlayerUpdateCoinsValue += UpdateCoinsUI;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerGetUserIDFromDB -= ShowLobby;
        EventBus.OnPlayerUpdateCoinsValue -= UpdateCoinsUI;
    }

    private void Start()
    {
        if(ConnectDatabase.IsUserEnter)
        {
            _registrationPanel.SetActive(false);
            _lobby.SetActive(true);

            LoadData.Instance.LoadUserData(_character);
            Invoke(nameof(UpdateInfo), 1f);
        }

        LoadingUI.Hide();
    }

    private void UpdateCoinsUI(int newValue, bool anim)
    {
        int differenceValue = newValue - _coins;

        if (_coinsText != null)
        {
            if (anim)
            {
                _differenceText.text = $"{(differenceValue > 0 ? '+' : ' ')}{differenceValue}";
                _coinsText.DOCounter(_coins, newValue, 2f);
                _animator.SetTrigger("update");
            }
            else
            {
                _coinsText.text = newValue.ToString();
            }
        }
        _coins = newValue;
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
