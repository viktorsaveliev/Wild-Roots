using TMPro;
using UnityEngine;

public class TopDayPlayer : MonoBehaviour
{
    [SerializeField] private GameObject _ui;
    [SerializeField] private TMP_Text _nickname;
    [SerializeField] private TMP_Text _prize;

    [SerializeField] private AudioClip _audio;

    private void OnEnable()
    {
        EventBus.OnPlayerTopTodayWinner += ShowWinnerScreen;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerTopTodayWinner -= ShowWinnerScreen;
    }

    private void ShowWinnerScreen(string nickname, int prize)
    {
        _ui.SetActive(true);
        _nickname.text = nickname;
        _prize.text = prize.ToString();

        StringBus stringBus = new();
        if(PlayerPrefs.GetInt(stringBus.SettingsSoundFX) == 0)
        {
            AudioSource.PlayClipAtPoint(_audio, Vector3.zero);
        }
    }

    public void HideScreen()
    {
        _ui.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }
}
