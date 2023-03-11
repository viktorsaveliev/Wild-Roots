using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Nickname : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nickname;

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.NickName == string.Empty)
        {
            StringBus stringBus = new();
            string nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];
            PhotonNetwork.LocalPlayer.NickName = _nickname.text = nickname;
        }
        else SetNickname();
    }

    private void OnEnable()
    {
        EventBus.OnPlayerChangeNickname += SetNickname;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerChangeNickname -= SetNickname;
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
            Notice.ShowDialog(NoticeDialog.Message.InvalidNickname);
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        StringBus stringBus = new();
        if (PlayerPrefs.GetInt(stringBus.GuestAcc) == 1)
        {
            PhotonNetwork.LocalPlayer.NickName = _nickname.text;
            EventBus.OnPlayerClickUI?.Invoke(2);
        }
        else
        {
            StartCoroutine(ChangeNickname(_nickname.text));
        }
    }

    private IEnumerator ChangeNickname(string nickname)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("nickname", nickname);
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.PlayerID));

        using UnityWebRequest www = UnityWebRequest.Post("https://www.wildroots.fun/public/update_nickname.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            _nickname.text = PhotonNetwork.LocalPlayer.NickName;
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            EventBus.OnPlayerClickUI?.Invoke(3);
        }
        else
        {
            bool successful = bool.Parse(www.downloadHandler.text);
            if (successful)
            {
                PhotonNetwork.LocalPlayer.NickName = _nickname.text;
                EventBus.OnPlayerClickUI?.Invoke(2);
                EventBus.OnPlayerChangeNickname?.Invoke();
            }
            else
            {
                _nickname.text = PhotonNetwork.LocalPlayer.NickName;
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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
            if (lowerCaseMessage.Contains(word))
            {
                return false;
            }
        }
        return true;
    }
}
