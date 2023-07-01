using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;

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
    [SerializeField] private GameObject _matchHandler;

    private ConnectHandler _connectHandler;
    private PhotonView _photonView;

    //private Coroutine _secTimer;
    private Coroutine _waitingTimer;

    private bool _startMatch;

    //private readonly int _secToFindPlayers = 60;
    //private int _currentSecToFindPlayers;

    private void Start()
    {
        _startMatch = false;

        _connectHandler = GetComponent<ConnectHandler>();
        _photonView = GetComponent<PhotonView>();
    }

    public void CreateRoom(bool open)
    {
        if (PhotonNetwork.InRoom) return;
        string roomName = GenerateRoomName(6);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 3, CleanupCacheOnLeave = false, IsOpen = open, IsVisible = true });
    }

    private void Play()
    {
        /*if (_isJoiningRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DOTween.Clear();
                StartGame();
            }
            return;
        }
        _isJoiningRoom = true;*/
        //PhotonNetwork.JoinRandomRoom();

        LoadingUI.Show(LoadingShower.Type.Simple);
        _startMatch = true;

        if (PhotonNetwork.InRoom == false)
        {
            CreateRoom(false);
        }
        else
        {
            StartGame();
        }
    }

    public void ConnectToMatch()
    {
        EventBus.OnPlayerClickUI?.Invoke(0);

        if (!_connectHandler.IsConnected)
        {
            _connectHandler.Connect();
            //Notice.ShowDialog(NoticeDialog.Message.ConnectionError, this, "Notice_Retry", "Notice_Close");
            if(_waitingTimer != null)
            {
                StopCoroutine(_waitingTimer);
                LoadingUI.Hide();
            }
            _waitingTimer = StartCoroutine(WaitingForConnection(5));
            return;
        }
        Play();
    }

    public void ActionOnClickNotice(int button)
    {
        if(button == 0)
        {
            if (!_connectHandler.IsConnected)
            {
                _connectHandler.Connect();
            }
            ConnectToMatch();
        }
        Notice.HideDialog();
    }

    [PunRPC]
    public void TakeFreeSlot(string nickname, int skinID)
    {
        if (_lobbySlots.Length > 0)
        {
            bool full = false;
            foreach (LobbySlot lobbySlot in _lobbySlots)
            {
                if (lobbySlot.IsUsed) continue;

                lobbySlot.TakeSlot(nickname, skinID);

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
                Play();
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
            CreateRoom(true);
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
        if (_startMatch) StartGame();
        else
        {
            _photonView.RPC(nameof(TakeFreeSlot), RpcTarget.OthersBuffered, PlayerData.GetNickname(), PlayerData.GetSkinID());
        }

        /*//Invoke(nameof(ActivateSearchScreen), 1f);
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
        _secTimer = StartCoroutine(SecTimer());*/
    }

    private void ActivateSearchScreen()
    {
        _searchScreen.SetActive(true);
        SaveData.Instance.Stats(SaveData.Statistics.SearchMatch);
    }
    
    /*private IEnumerator SecTimer()
    {
        while (_currentSecToFindPlayers > 0)
        {
            yield return new WaitForSeconds(1f);
            _currentSecToFindPlayers--;
            if (PhotonNetwork.IsMasterClient)
            {
                if (_currentSecToFindPlayers <= 0)
                {
                    StartGame();
                    break;
                }
                else _photonView.RPC(nameof(UpdateWaitingUI), RpcTarget.All, _currentSecToFindPlayers);
            }
        }
    }*/

    /*public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene((int)GameSettings.Scene.Lobby);
    }*/

    public void StartGame()
    {
        _photonView.RPC(nameof(LoadLevelForAll), RpcTarget.All);
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    [PunRPC]
    public void LoadLevelForAll()
    {
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel((int)GameSettings.Scene.MatchInformer);
            yield return PhotonNetwork.InstantiateRoomObject(_matchHandler.name, Vector3.zero, Quaternion.identity);
        }
        Match.Init();

        while (PhotonNetwork.LevelLoadingProgress < 1f)
        {
            LoadingUI.UpdateProgress(PhotonNetwork.LevelLoadingProgress);
            yield return new WaitForEndOfFrame();
        }
    }
    
    /*[PunRPC]
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
    }*/

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
