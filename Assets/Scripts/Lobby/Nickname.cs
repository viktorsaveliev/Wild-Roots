using Photon.Pun;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Nickname : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nickname;

    private void Start()
    {
        if (PlayerData.GetNickname() == string.Empty)
        {
            StringBus stringBus = new();
            string nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];
            PlayerData.SetNickname(nickname);
            _nickname.text = nickname;
        }
        else SetNickname();
    }

    private void OnEnable()
    {
        EventBus.OnPlayerChangeNickname += SetNickname;
        EventBus.OnPlayerTryChangeNickname += CheckNickname;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerChangeNickname -= SetNickname;
        EventBus.OnPlayerTryChangeNickname -= CheckNickname;
    }

    public void ShowKeyboard()
    {
        Keyboard.Show(_nickname, true);
    }

    private void SetNickname()
    {
        _nickname.text = PhotonNetwork.LocalPlayer.NickName;
    }

    public void CheckNickname()
    {
        if (PhotonNetwork.LocalPlayer.NickName == _nickname.text) return;

        if(_nickname.text == string.Empty || _nickname.text.Length < 4 || _nickname.text.Length > 24 || AreYouCute(_nickname.text) == false)
        {
            _nickname.text = PhotonNetwork.LocalPlayer.NickName;
            Notice.Dialog(NoticeDialog.Message.InvalidNickname);
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        StringBus stringBus = new();
        if (PlayerPrefs.GetInt(stringBus.IsGuest) == 1)
        {
            PlayerData.SetNickname(_nickname.text);
            EventBus.OnPlayerClickUI?.Invoke(2);
        }
        else
        {
            StartCoroutine(ChangeNickname(_nickname.text));
        }
    }

    public void CheckNickname(string nickname)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nickname) return;

        if (nickname == string.Empty || nickname.Length < 4 || nickname.Length > 24 || AreYouCute(nickname) == false)
        {
            _nickname.text = PhotonNetwork.LocalPlayer.NickName;
            Notice.Dialog(NoticeDialog.Message.InvalidNickname);
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        StringBus stringBus = new();
        if (PlayerPrefs.GetInt(stringBus.IsGuest) == 1)
        {
            PlayerData.SetNickname(nickname);
            EventBus.OnPlayerClickUI?.Invoke(2);
        }
        else
        {
            StartCoroutine(ChangeNickname(nickname));
        }
    }

    private IEnumerator ChangeNickname(string nickname)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("nickname", nickname);
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));

        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "update_nickname.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            _nickname.text = PhotonNetwork.LocalPlayer.NickName;
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
            EventBus.OnPlayerClickUI?.Invoke(3);
        }
        else
        {
            bool successful = bool.Parse(www.downloadHandler.text);
            if (successful)
            {
                PlayerData.SetNickname(_nickname.text);
                EventBus.OnPlayerClickUI?.Invoke(2);
                EventBus.OnPlayerChangeNickname?.Invoke();
            }
            else
            {
                _nickname.text = PlayerData.GetNickname();
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
                EventBus.OnPlayerClickUI?.Invoke(3);
            }
        }
    }

    private bool AreYouCute(string message)
    {
        StringBus stringBus = new();
        string lowerCaseMessage = message.ToLower();

        foreach (string word in stringBus.YouNeedBeCute)
        {
            string pattern = @"\b" + word + @"\b";
            if (Regex.IsMatch(lowerCaseMessage, pattern))
            {
                return false;
            }
        }
        return true;
        /*StringBus stringBus = new();
        string lowerCaseMessage = message.ToLower();
        foreach (string word in stringBus.YouNeedBeCute)
        {
            if (lowerCaseMessage.Contains(word))
            {
                return false;
            }
        }
        return true;*/
    }
}
