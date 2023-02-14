using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(ConnectHandler), typeof(PhotonView))]
public class JoinRoomHandler : MonoBehaviourPunCallbacks, INoticeAction
{
    [SerializeField] private GameObject _mainLobby;
    [SerializeField] private GameObject _searchScreen;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private Text _textWaiting;
    [SerializeField] private LobbySlot[] _lobbySlots;
    [SerializeField] private GameObject _character;

    private bool _isJoiningRoom = false;
    private ConnectHandler _connectHandler;
    private PhotonView _photonView;

    private Coroutine _secTimer;
    private readonly int _secToFindPlayers = 90;
    private int _currentSecToFindPlayers;

    private void Start()
    {
        _connectHandler = GetComponent<ConnectHandler>();
        _photonView = GetComponent<PhotonView>();
    }

    private void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5, CleanupCacheOnLeave = false, IsOpen = true, IsVisible = true });
        _currentSecToFindPlayers = _secToFindPlayers;

        if (_secTimer != null) StopCoroutine(_secTimer);
        _secTimer = StartCoroutine(SecTimer());

        //_connectHandler.Log("Created new room..");
    }

    public void JoinRoom()
    {
        EventBus.OnPlayerClickUI?.Invoke(0);
        if(!_connectHandler.IsConnected)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError, this, "Notice_Retry", "Notice_Close");
            return;
        }
        if (_isJoiningRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DOTween.Clear();
                StartGame();
            }
            return;
        }

        LoadingUI.Show(LoadingShower.Type.Simple);

        PhotonNetwork.JoinRandomRoom();
        _isJoiningRoom = true;
    }

    public void ActionOnClickNotice(int button)
    {
        if(button == 0)
        {
            JoinRoom();
        }
        Notice.HideDialog();
    }

    [PunRPC]
    public void TakeFreeSlot()
    {
        if (_lobbySlots.Length > 0)
        {
            bool full = false;
            foreach (LobbySlot lobbySlot in _lobbySlots)
            {
                if (lobbySlot.IsUsed) continue;

                lobbySlot.TakeSlot("Player", 0);

                full = true;
                break;
            }
            if (!full)
            {
                _connectHandler.Log("Cant found free slot for you :(");
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (returnCode == 32760) // No available rooms found!
        {
            CreateRoom();
        }
        else if(returnCode == 32757) // MaxCCU
        {
            Notice.ShowDialog(NoticeDialog.Message.ServerFull);
        }
        else
        {
            LoadingUI.Hide();
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public override void OnJoinedRoom()
    {
        LoadingUI.Hide();

        Invoke(nameof(ActivateSearchScreen), 1f);
        EventBus.OnPlayerStartSearchMatch?.Invoke();

        _photonView.RPC(nameof(TakeFreeSlot), RpcTarget.OthersBuffered);
        _character.transform.DOScale(4.5f, 1f);
        _character.transform.DOMoveY(0.14f, 1f);

        if(PhotonNetwork.IsMasterClient)
        {
            _startButton.SetActive(true);
        }
    }

    private void ActivateSearchScreen() => _searchScreen.SetActive(true);
    private IEnumerator SecTimer()
    {
        while (_currentSecToFindPlayers > 0)
        {
            yield return new WaitForSeconds(1f);
            if (PhotonNetwork.IsMasterClient)
            {
                if (--_currentSecToFindPlayers <= 0)
                {
                    StartGame();
                    break;
                }
                _photonView.RPC(nameof(UpdateWaitingUI), RpcTarget.All);
            }
        }
    }

    private void StartGame()
    {
        _photonView.RPC(nameof(LoadLevelProgressForAll), RpcTarget.All);

        StopCoroutine(_secTimer);
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    [PunRPC]
    public void LoadLevelProgressForAll()
    {
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        if(PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(1); // game
        LoadingUI.Show(LoadingShower.Type.Progress);

        while (PhotonNetwork.LevelLoadingProgress < 1f)
        {
            LoadingUI.UpdateProgress(PhotonNetwork.LevelLoadingProgress);
            yield return new WaitForEndOfFrame();
        }
    }
    
    [PunRPC]
    public void UpdateWaitingUI()
    {
        _textWaiting.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && _currentSecToFindPlayers > 3) // 
        {
            _currentSecToFindPlayers = 3;
        }
    }
}
