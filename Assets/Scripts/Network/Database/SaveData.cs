using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SaveData : MonoBehaviour
{
    [SerializeField] private GameObject _saveDataIndicator;

    public static SaveData Instance;

    private void Awake()
    {
        if (!LoadingShower.IsCreated) Instance = this;
    }

    public void SaveNewPassword(string password)
    {
        StartCoroutine(IESaveNewPassword(password));
    }

    private IEnumerator IESaveNewPassword(string password)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("password", password);
        form.AddField("email", PlayerPrefs.GetString("email"));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "update_password.php", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
            else
            {
                Notice.Dialog(NoticeDialog.Message.PasswordChanged);
            }
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        PlayerPrefs.DeleteKey("email");
    }

    public void SaveProgress(int playerID)
    {
        StartCoroutine(SaveProgressAsync(playerID));
    }

    private IEnumerator SaveProgressAsync(int playerID)
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;
        if (isGuest)
        {
            yield break;
        }

        if (_saveDataIndicator != null) _saveDataIndicator.SetActive(true);
        WWWForm form = new();
        form.AddField("ID", playerID);
        form.AddField("level", PlayerData.GetLevel());
        form.AddField("exp", PlayerData.GetExp());
        form.AddField("wins", PlayerData.GetWins());
        form.AddField("wins_today", PlayerData.GetWinsToday());
        form.AddField("adsViewed", PlayerData.GetWatchedAds());
        
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "update_progress.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        if (_saveDataIndicator != null) _saveDataIndicator.SetActive(false);
    }

    public IEnumerator Coins(int value)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("coins", value);
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "save_coins.php", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void Skin()
    {
        StartCoroutine(SkinID());
    }

    private IEnumerator SkinID()
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("skinID", PlayerData.GetSkinID());
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "save_skin_id.php", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void AdsCount()
    {
        StartCoroutine(AdsCountAsync());
    }

    private IEnumerator AdsCountAsync()
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "watched_ads.php", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.Dialog(www.downloadHandler.text);
        }
    }

    public enum Statistics
    {
        Enter,
        Registration,
        Login,
        PlayAsGuest,
        MatchStart,
        MatchEnd,
        MidgameAds,
        RewardAds,
        LeftTheMatch,
        Desktop,
        Mobile,
        SearchMatch,
        BuyNewSkin
    }

    public void Stats(Statistics stats)
    {
        StartCoroutine(SaveStats(stats));
    }

    private IEnumerator SaveStats(Statistics stats)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("id", (int)stats);
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "update_stats.php", form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }
}
