using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ConnectHandler), typeof(PhotonView))]
public class JoinRoomHandler : MonoBehaviourPunCallbacks, INoticeAction
{
    [SerializeField] private GameObject _mainLobby;
    [SerializeField] private GameObject _searchScreen;
    [SerializeField] private GameObject _startButton;

    [SerializeField] private Text _textWaiting;
    [SerializeField] private Text _timerCount;

    [SerializeField] private LobbySlot[] _lobbySlots;
    [SerializeField] private GameObject _character;

    private bool _isJoiningRoom = false;
    private ConnectHandler _connectHandler;
    private PhotonView _photonView;

    private Coroutine _secTimer;
    private readonly int _secToFindPlayers = 60;
    private int _currentSecToFindPlayers;

    private GameModeSelector.GameMode _selectedGameMode;

    private void Start()
    {
        _connectHandler = GetComponent<ConnectHandler>();
        _photonView = GetComponent<PhotonView>();
    }

    private void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5, CleanupCacheOnLeave = false, IsOpen = true, IsVisible = true });
        //_currentSecToFindPlayers = _secToFindPlayers;

        //if (_secTimer != null) StopCoroutine(_secTimer);
        //_secTimer = StartCoroutine(SecTimer());

        //_connectHandler.Log("Created new room..");
    }

    private void Play(GameModeSelector.GameMode gamemode)
    {
        if(gamemode == GameModeSelector.GameMode.PvP)
        {
            if (_isJoiningRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    DOTween.Clear();
                    StartGame(GameModeSelector.GameMode.PvP);
                }
                return;
            }
            LoadingUI.Show(LoadingShower.Type.Simple);
            PhotonNetwork.JoinRandomRoom();

            _isJoiningRoom = true;
        }
        else if(gamemode == GameModeSelector.GameMode.Deathmatch)
        {
            if (_isJoiningRoom) return;

            LoadingUI.Show(LoadingShower.Type.Simple);
            PhotonNetwork.JoinRoom("Deathmatch");

            _isJoiningRoom = true;
        }
    }

    public void SelectMode(int modeID)
    {
        EventBus.OnPlayerClickUI?.Invoke(0);

        _selectedGameMode = (GameModeSelector.GameMode)modeID;
        if (!_connectHandler.IsConnected)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError, this, "Notice_Retry", "Notice_Close");
            return;
        }

        GameSettings.GameMode = _selectedGameMode;
        Play(GameSettings.GameMode);
    }

    public void ActionOnClickNotice(int button)
    {
        if(button == 0)
        {
            SelectMode((int)_selectedGameMode);
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
                print("Cant found free slot for you :(");
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode == 32758) // Game does not exist
        {
            PhotonNetwork.CreateRoom("Deathmatch", new RoomOptions { MaxPlayers = 20, CleanupCacheOnLeave = false, IsOpen = true, IsVisible = false });
        }
        else if (returnCode == 32757) // MaxCCU
        {
            Notice.ShowDialog(NoticeDialog.Message.ServerFull);
        }
        else
        {
            LoadingUI.Hide();
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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

        if(GameSettings.GameMode == GameModeSelector.GameMode.PvP)
        {
            Invoke(nameof(ActivateSearchScreen), 1f);
            EventBus.OnPlayerStartSearchMatch?.Invoke();

            _photonView.RPC(nameof(TakeFreeSlot), RpcTarget.OthersBuffered);
            _character.transform.DOScale(4.5f, 1f);
            _character.transform.DOMoveY(0.14f, 1f);

            if (PhotonNetwork.IsMasterClient)
            {
                _startButton.SetActive(true);
            }

            _currentSecToFindPlayers = _secToFindPlayers;
            if (_secTimer != null) StopCoroutine(_secTimer);
            _secTimer = StartCoroutine(SecTimer());
        }
        else if (GameSettings.GameMode == GameModeSelector.GameMode.Deathmatch)
        {
            StartGame(GameModeSelector.GameMode.Deathmatch);
        }
    }

    private void ActivateSearchScreen() => _searchScreen.SetActive(true);
    private IEnumerator SecTimer()
    {
        while (_currentSecToFindPlayers > 0)
        {
            yield return new WaitForSeconds(1f);
            _currentSecToFindPlayers--;
            if (PhotonNetwork.IsMasterClient)
            {
                if (_currentSecToFindPlayers <= 0)
                {
                    /*if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
                    {
                        /*_character.transform.DOScale(5f, 1f);
                        _character.transform.DOMoveY(0.84f, 1f);
                        _mainLobby.SetActive(true);
                        _searchScreen.SetActive(false);
                        PhotonNetwork.LeaveRoom();
                        Notice.ShowDialog(NoticeDialog.Message.EmptyQueue);
                    }
                    else
                    {*/
                    StartGame(GameModeSelector.GameMode.PvP);
                    break;
                }
                else _photonView.RPC(nameof(UpdateWaitingUI), RpcTarget.All, _currentSecToFindPlayers);
            }
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }

    public void StartGame(GameModeSelector.GameMode gamemode)
    {
        if (gamemode == GameModeSelector.GameMode.PvP)
        {
            _photonView.RPC(nameof(LoadLevelProgressForAll), RpcTarget.All);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            StopCoroutine(_secTimer);
        }
        else
        {
            LoadingUI.Show(LoadingShower.Type.Progress);
            SceneManager.LoadScene(1); // game
        }
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
    public void UpdateWaitingUI(int time)
    {
        _currentSecToFindPlayers = time;
        if (_currentSecToFindPlayers > 59) _timerCount.text = $"1:{_currentSecToFindPlayers - 60}";
        else if (_currentSecToFindPlayers > 9) _timerCount.text = $"0:{_currentSecToFindPlayers}";
        else _timerCount.text = $"0:0{_currentSecToFindPlayers}";

        _textWaiting.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && _currentSecToFindPlayers > 3) // 
        {
            _currentSecToFindPlayers = 3;
        }
    }
}
