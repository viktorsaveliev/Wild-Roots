using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ConnectHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text _textVerison;

    public bool IsConnected { get; private set; }

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = BaseGameData.GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        _textVerison.text = BaseGameData.GameVersion;
    }

    public override void OnConnectedToMaster()
    {
        IsConnected = true;
        print($"Connected to region: {PhotonNetwork.CloudRegion}");
        print($"Players online: {PhotonNetwork.CountOfPlayers}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        IsConnected = false;
    }
}
