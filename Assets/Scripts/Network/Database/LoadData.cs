using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using CrazyGames;
using Photon.Pun;

public class LoadData : MonoBehaviour
{
    [SerializeField] private GameObject _regauthPanel;

    public static LoadData Instance;

    private void Start()
    {
        Instance = this;
    }

    public void LoadLevelData(Character player)
    {
        StartCoroutine(IELoadLevelData(player));
    }

    private IEnumerator IELoadLevelData(Character player)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.PlayerID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "get_data.php", form);
        yield return www.SendWebRequest();

        // Handle JSON response
        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonString = www.downloadHandler.text;
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonString);

            player.Level = playerData.level;
            player.Exp = playerData.exp;
            player.Wins = playerData.wins;
            player.Nickname = playerData.nickname;

            PhotonNetwork.LocalPlayer.NickName = player.Nickname;
            EventBus.OnPlayerChangeNickname?.Invoke();
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void LoadAccount(string email, string password, bool remember)
    {
        StringBus stringBus = new();
        PlayerPrefs.DeleteKey(stringBus.GuestAcc);
        StartCoroutine(IELoadAccount(email, password, remember));
    }

    private IEnumerator IELoadAccount(string email, string password, bool remember)
    {
        _regauthPanel.SetActive(false);

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
            PlayerPrefs.SetInt(stringBus.PlayerID, playerID);
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

        EventBus.OnPlayerLoadAccount?.Invoke();
    }
}

[System.Serializable]
public class PlayerData
{
    public int level;
    public int exp;
    public int wins;
    public string nickname;
}
