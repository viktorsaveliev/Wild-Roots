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
        NewLevel,
        ForKiller
    }

    public void GiveCoins(GiveReason reason)
    {
        switch (reason)
        {
            case GiveReason.Ads:
                PlayerData.ViewedAds();
                StartCoroutine(GiveCoinsForAds());
                break;

            case GiveReason.ForKiller:
                StartCoroutine(GiveCoinsForKiller());
                break;

            default:
                break;
        }
        
    }

    private IEnumerator GiveCoinsForKiller()
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;
        if (isGuest)
        {
            UpdateValue(_coins + PlayerData.GetDroppedPlayersCount * 15, true);
            PlayerData.SetDroppedPlayersCount(0);
            yield break;
        }

        WWWForm form = new();
        form.AddField("id", PlayerPrefs.GetInt(stringBus.UserID));
        form.AddField("kills", PlayerData.GetDroppedPlayersCount);

        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "give_coins_killer.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = int.TryParse(www.downloadHandler.text, out int value);
            if (success)
            {
                UpdateValue(_coins + PlayerData.GetDroppedPlayersCount * value, true);
                PlayerData.SetDroppedPlayersCount(0);
            }
            else
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            print(www.downloadHandler.error);
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    private IEnumerator GiveCoinsForAds()
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

        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "give_coins_ads.php", form);
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
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            print(www.downloadHandler.error);
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
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
