using UnityEngine;
using CrazyGames;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using TMPro;

public class InviteLink : MonoBehaviour, INoticeAction
{
    [SerializeField] private JoinRoomHandler _joinRoom;
    [SerializeField] private GameObject _writeRoomCodeUI;
    [SerializeField] private TMP_InputField _roomCode;

    public void ShowRoomCode()
    {
        StartCoroutine(RoomCode());
    }

    private IEnumerator RoomCode()
    {
        if (PhotonNetwork.InRoom == false)
        {
            LoadingUI.Show(LoadingShower.Type.Simple);
            _joinRoom.CreateRoom(true);
        }

        while (PhotonNetwork.InRoom == false)
        {
            yield return null;
        }
        LoadingUI.Hide();

        Notice.Dialog($"Room code: {PhotonNetwork.CurrentRoom.Name}", this, "Copy_RoomButton", "Join_RoomButton");
    }

    private void CopyLinkToClipboard()
    {
        Dictionary<string, string> parameters = new()
        {
            { "roomId", PhotonNetwork.CurrentRoom.Name }
        };

        string inviteLink = CrazyEvents.Instance.InviteLink(parameters);
        CrazyEvents.Instance.CopyToClipboard(inviteLink);

        Notice.Simple(NoticeDialog.Message.Simple_CopyToClipboard, true);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    public void ActionOnClickNotice(int button)
    {
        if (button == 0)
        {
            CopyLinkToClipboard();
        }
        else
        {
            _writeRoomCodeUI.SetActive(true);
        }
        EventBus.OnPlayerClickUI?.Invoke(1);
        Notice.HideDialog();
    }

    public void ConnectRoom() => StartCoroutine(ConnectToRoom());
    private IEnumerator ConnectToRoom()
    {
        if (_roomCode.text.Length < 6) yield break;

        yield return Disconnect();

        PhotonNetwork.JoinRoom(_roomCode.text);
        _roomCode.text = string.Empty;
        _writeRoomCodeUI.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    private IEnumerator Disconnect()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
    }

    public void Close()
    {
        _roomCode.text = string.Empty;
        _writeRoomCodeUI.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    public void ShowKeyboard()
    {
        Keyboard.Show(_roomCode);
    }
}