using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using CrazyGames;

public class ConnectHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text _textVerison;
    [SerializeField] private GameObject _guestNote;

    public bool IsConnected { get; private set; }

    private void Start()
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;

        if (isGuest)
        {
            _guestNote.SetActive(true);
        }

        Connect();
        /*bool isInviteLink = CrazyEvents.Instance.IsInviteLink();
        if (isInviteLink)
        {
            //Connect();
        }*/

        _textVerison.text = BaseGameData.GameVersion;
        EventBus.OnPlayerLogged?.Invoke();
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = BaseGameData.GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        IsConnected = true;

        print($"Connected to region: {PhotonNetwork.CloudRegion}");
        print($"Players online: {PhotonNetwork.CountOfPlayers}");

        bool isInviteLink = CrazyEvents.Instance.IsInviteLink();
        if(isInviteLink)
        {
            string roomId = CrazyEvents.Instance.GetInviteLinkParameter("roomId");

            if (roomId == null && roomId == "")
            {
                print("[ERROR]: Inactive invite link");
                return;
            }

            PhotonNetwork.JoinRoom(roomId);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        IsConnected = false;
    }
}
