using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;

public class CoinsHandler : MonoBehaviour
{
    [SerializeField] private Text _coinsText;
    [SerializeField] private Text _differenceText;
    [SerializeField] private Animator _animator;

    private int _coins;

    public enum GiveReason
    {
        Ads,
        Winner,
        NewLevel
    }

    public void GiveCoins(GiveReason reason)
    {
        StartCoroutine(IEGiveCoins(reason));
    }

    private IEnumerator IEGiveCoins(GiveReason reason)
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;
        if (isGuest)
        {
            UpdateUI(_coins + 100);
            yield break;
        }

        WWWForm form = new();
        form.AddField("id", PlayerPrefs.GetInt(stringBus.UserID));

        string scriptName;
        switch (reason)
        {
            case GiveReason.Ads:
                scriptName = "give_coins_ads.php";
                break;

            default:
                scriptName = "give_coins_ads.php";
                break;
        }

        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + scriptName, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = int.TryParse(www.downloadHandler.text, out int value);
            if (success)
            {
                UpdateUI(value);
            }
            else
            {
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            print(www.downloadHandler.error);
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void UpdateValue(int newValue, bool anim = true)
    {
        _coins = newValue;
        UpdateUI(_coins, anim);
    }

    private void UpdateUI(int newValue, bool anim = true)
    {
        int differenceValue = newValue - _coins;
        if(_coinsText != null)
        {
            if(anim)
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
    }

    public void Pay(int value)
    {
        UpdateUI(_coins - value);

        _coins -= value;

        if (_coins < 0)
        {
            _coins = 0;
            print("Error #002: Coins have wrong value!");
        }

        StartCoroutine(SaveData.Instance.Coins(_coins));
    }

    public int GetValue() => _coins;
}
