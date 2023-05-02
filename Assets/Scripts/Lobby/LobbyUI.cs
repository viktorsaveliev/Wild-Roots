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

    private int _coins = 0;

    private void OnEnable()
    {
        EventBus.OnPlayerLogged += UpdateInfoWithDelay;
        EventBus.OnPlayerUpdateCoinsValue += UpdateCoinsUI;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerLogged -= UpdateInfoWithDelay;
        EventBus.OnPlayerUpdateCoinsValue -= UpdateCoinsUI;
    }

    private void Start()
    {
        if(ConnectDatabase.IsUserEnter)
        {
            UpdateInfoWithDelay();
        }
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
                _coinsText.DOCounter(_coins, newValue, 2f);
                //_coinsText.text = newValue.ToString();
            }
        }
        _coins = newValue;
    }

    private void UpdateInfo()
    {
        //StringBus stringBus = new();
        //if (PlayerPrefs.GetInt(stringBus.GuestAcc) == 1) return;

        if (PlayerData.GetLevel() == 0) PlayerData.Update(string.Empty, 1, 0, 0, 0, 0, 0);

        int maxLevelExp = PlayerData.GetLevel() * 3 * 100;
        _level.text = $"{PlayerData.GetLevel()}";
        _levelExp.text = $"{PlayerData.GetExp()}/{maxLevelExp}";

        float exp = PlayerData.GetExp() + 0.01f;
        _levelExpProgressBar.fillAmount = exp / maxLevelExp;

        _winsCount.text = $"{PlayerData.GetWins()}";

        UpdateCoinsUI(Coins.GetValue(), false);
    }

    private void UpdateInfoWithDelay()
    {
        Invoke(nameof(UpdateInfo), 0.5f);
    }
}
