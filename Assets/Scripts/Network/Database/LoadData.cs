using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using CrazyGames;
using Photon.Pun;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LoadData : MonoBehaviour
{
    public static LoadData Instance;

    private void Start()
    {
        if (!LoadingShower.IsCreated) Instance = this;
    }

    public void LoadUserData(Character character)
    {
        StartCoroutine(LoadUserDataAsync(character));
    }

    public IEnumerator LoadUserDataAsync(Character character)
    {
        StringBus stringBus = new();

        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;
        if (isGuest)
        {
            yield break;
        }

        WWWForm form = new();
        form.AddField("id", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "get_data.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            UpdateUser(www.downloadHandler.text);
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void UpdateUser(string json)
    {
        string jsonString = json;
        PlayerInfo playerData = JsonConvert.DeserializeObject<PlayerInfo>(jsonString);

        StringBus stringBus = new();

        PlayerData.Update(playerData.nickname, playerData.level, playerData.exp, playerData.wins, playerData.wins_today, playerData.adsViewed, playerData.skinID);
        Coins.UpdateValue(playerData.coins, false);

        if (playerData.today_winner > 0)
        {
            StartCoroutine(ShowAndResetTodayWinner(playerData.nickname, playerData.today_winner));
        }
        PlayerPrefs.SetInt(stringBus.UserID, playerData.ID);

        PhotonNetwork.LocalPlayer.NickName = playerData.nickname;
        EventBus.OnPlayerChangeNickname?.Invoke();
    }

    private IEnumerator ShowAndResetTodayWinner(string nickname, int coins)
    {
        yield return new WaitForSeconds(1.5f);
        CrazyEvents.Instance.HappyTime();
        EventBus.OnPlayerTopTodayWinner?.Invoke(nickname, coins);

        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "reset_today_winner.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(www.downloadHandler.error);
        }
    }

    public void GetUserID(string email, string password, bool remember)
    {
        StringBus stringBus = new();
        PlayerPrefs.DeleteKey(stringBus.IsGuest);
        StartCoroutine(GetUserIDAsync(email, password, remember));
    }

    private IEnumerator GetUserIDAsync(string email, string password, bool remember)
    {
        LoadingUI.Show(LoadingShower.Type.Simple);

        StringBus stringBus = new();

        if (remember)
        {
            PlayerPrefs.SetInt(stringBus.AccStatus, 2);
            PlayerPrefs.SetString(stringBus.Email, email);
            PlayerPrefs.SetString(stringBus.Password, password);
        }
        else
        {
            PlayerPrefs.SetInt(stringBus.AccStatus, 1);
            PlayerPrefs.DeleteKey(stringBus.Email);
            PlayerPrefs.DeleteKey(stringBus.Password);
        }

        WWWForm form = new();
        form.AddField("email", email);
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "get_id.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            int playerID = int.Parse(www.downloadHandler.text);
            PlayerPrefs.SetInt(stringBus.UserID, playerID);
            PlayerData.Update(string.Empty, 1, 0, 0, 0, 0, 1);
        }
        else
        {
            Notice.Dialog(www.downloadHandler.error);
        }

        CrazySDK.Instance.GetUserInfo(userInfo =>
        {
            int deviceType = 0;
            if (userInfo.device.type == "desktop" || userInfo.browser.name == "demo")
            {
                deviceType = 1;
            }
            PlayerPrefs.SetInt(stringBus.PlayerDevice, deviceType);
        });

        PlayerPrefs.Save();

        DOTween.Clear();
        ConnectDatabase.IsUserEnter = true;
        SceneManager.LoadSceneAsync((int)GameSettings.Scene.Lobby);
        //EventBus.OnPlayerGetUserIDFromDB?.Invoke();
    }
}

[System.Serializable]
public class PlayerInfo
{
    public int ID;
    public int level;
    public int exp;
    public int wins;
    public int wins_today;
    public int today_winner;
    public string nickname;
    public int coins;
    public int skinID;
    public int adsViewed;
}
