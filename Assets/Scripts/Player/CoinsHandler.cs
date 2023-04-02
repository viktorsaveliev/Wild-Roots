using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CoinsHandler : MonoBehaviour
{
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
            UpdateValue(_coins + 100, true);
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
                UpdateValue(value, true);
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
        EventBus.OnPlayerUpdateCoinsValue?.Invoke(_coins, anim);
    }

    public void Pay(int value)
    {
        EventBus.OnPlayerUpdateCoinsValue?.Invoke(_coins - value, true);

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
