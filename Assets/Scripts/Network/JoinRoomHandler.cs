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
        string roomName = GenerateRoomName(6);
//#if (!UNITY_EDITOR)
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 5, CleanupCacheOnLeave = false, IsOpen = true, IsVisible = true });
//#else
        //PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 5, CleanupCacheOnLeave = false, IsOpen = false, IsVisible = true });
//#endif
        //_currentSecToFindPlayers = _secToFindPlayers;

        //if (_secTimer != null) StopCoroutine(_secTimer);
        //_secTimer = StartCoroutine(SecTimer());

        //_connectHandler.Log("Created new room..");
    }

    private void Play(GameModeSelector.GameMode gamemode)
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

    public void SelectMode(int modeID)
    {
        EventBus.OnPlayerClickUI?.Invoke(0);

        _selectedGameMode = (GameModeSelector.GameMode)modeID;
        if (!_connectHandler.IsConnected)
        {
            _connectHandler.Connect();
            //Notice.ShowDialog(NoticeDialog.Message.ConnectionError, this, "Notice_Retry", "Notice_Close");
            StartCoroutine(WaitingForConnection(5));
            return;
        }

        GameSettings.GameMode = _selectedGameMode;
        Play(GameSettings.GameMode);
    }

    public void ActionOnClickNotice(int button)
    {
        if(button == 0)
        {
            if (!_connectHandler.IsConnected)
            {
                _connectHandler.Connect();
            }
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
        /*if (returnCode == 32758) // Game does not exist
        {
            PhotonNetwork.CreateRoom("Deathmatch", new RoomOptions { MaxPlayers = 20, CleanupCacheOnLeave = false, IsOpen = true, IsVisible = false });
        }
        else */if (returnCode == 32757) // MaxCCU
        {
            LoadingUI.Hide();
            Notice.Dialog(NoticeDialog.Message.ServerFull);
        }
        else
        {
            LoadingUI.Hide();
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    private IEnumerator WaitingForConnection(int time)
    {
        LoadingUI.Show(LoadingShower.Type.Simple);

        int currentTimer = 0;
        while(currentTimer < time)
        {
            if(_connectHandler.IsConnected)
            {
                GameSettings.GameMode = _selectedGameMode;
                Play(GameSettings.GameMode);
                yield break;
            }
            currentTimer++;
            yield return new WaitForSeconds(1f);
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
            Notice.Dialog(NoticeDialog.Message.ServerFull);
        }
        else
        {
            LoadingUI.Hide();
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public override void OnJoinedRoom()
    {
        LoadingUI.Hide();

        //Invoke(nameof(ActivateSearchScreen), 1f);
        ActivateSearchScreen();

        EventBus.OnPlayerStartSearchMatch?.Invoke();

        //_photonView.RPC(nameof(TakeFreeSlot), RpcTarget.OthersBuffered);
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
        SceneManager.LoadScene((int)GameSettings.Scene.Lobby);
    }

    public void StartGame(GameModeSelector.GameMode gamemode)
    {
        if (gamemode == GameModeSelector.GameMode.PvP)
        {
            _photonView.RPC(nameof(LoadLevelProgressForAll), RpcTarget.All);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            StopCoroutine(_secTimer);
        }
    }

    [PunRPC]
    public void LoadLevelProgressForAll()
    {
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        if(PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel((int)GameSettings.Scene.Street); // game
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

    private const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private string GenerateRoomName(int length)
    {
        char[] password = new char[length];
        System.Random random = new();

        for (int i = 0; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }

        return new string(password);
    }
}
