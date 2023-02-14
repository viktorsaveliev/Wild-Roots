using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Text _level;
    [SerializeField] private Text _levelExp;
    [SerializeField] private Image _levelExpProgressBar;

    /*[SerializeField] private Text _battlePass;
    [SerializeField] private Text _battlePassExp;
    [SerializeField] private Image _battlePassExpProgressBar;*/

    [SerializeField] private Text _winsCount;

    [SerializeField] private GameObject _lobby;
    [SerializeField] private GameObject _selectDeviceScreen;
    public static bool IsGameLoaded = false;

    private void Start()
    {
        if(IsGameLoaded)
        {
            _selectDeviceScreen.SetActive(false);
            _lobby.SetActive(true);
        }
        else
        {
            IsGameLoaded = true;
        }

        UpdateInfo();
        LoadingUI.Hide();
    }

    private void UpdateInfo()
    {
        StringBus stringBus = new();

        int level = PlayerPrefs.GetInt(stringBus.PlayerLevel);
        float levelExp = PlayerPrefs.GetInt(stringBus.PlayerExp);
        int maxLevelExp = level * 3 * 100;
        int winsCount = PlayerPrefs.GetInt(stringBus.PlayerWinsCount);

        if (level == 0) level = 1;
        _level.text = $"{level}";
        _levelExp.text = $"{levelExp}/{maxLevelExp}";
        _levelExpProgressBar.fillAmount = levelExp / maxLevelExp;

        _winsCount.text = $"{winsCount}";
    }    
}
