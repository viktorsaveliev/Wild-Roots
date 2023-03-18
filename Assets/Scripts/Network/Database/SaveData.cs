using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SaveData : MonoBehaviour
{
    [SerializeField] private GameObject _saveDataIndicator;

    public static SaveData Instance;

    private void Start()
    {
        Instance = this;
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
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
            else
            {
                Notice.ShowDialog(NoticeDialog.Message.PasswordChanged);
            }
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
        PlayerPrefs.DeleteKey("email");
    }

    public void SaveLevelData(int playerID, Character player)
    {
        StartCoroutine(IESaveLevelData(playerID, player));
    }

    private IEnumerator IESaveLevelData(int playerID, Character player)
    {
        if(_saveDataIndicator != null) _saveDataIndicator.SetActive(true);
        WWWForm form = new();
        form.AddField("ID", playerID);
        form.AddField("level", player.Level);
        form.AddField("exp", player.Exp);
        form.AddField("wins", player.Wins);

        StringBus stringBus = new();
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "update_level.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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
        form.AddField("skinID", PlayerPrefs.GetInt(stringBus.SkinID));
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "save_skin_id.php", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }
}
