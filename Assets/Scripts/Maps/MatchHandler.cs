using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MatchHandler : MonoBehaviour
{
    private PhotonView _photonView;

    private int _currentPlayerID = -1;
    public int PlayersCount { get; private set; }

    private string[] _playersNicknames;
    public string[] GetPlayersNicknames => _playersNicknames;

    private int[] _playersCupsCount;
    public int[] GetPlayersCupsCount => _playersCupsCount;

    private int[] _playersSkinID;
    public int[] GetPlayersSkinID => _playersSkinID;
    public int CurrentRound { get; private set; }

    public int NextMap { get; private set; }
    private int[] _allMaps;
    private List<int> _availableMaps;

    private void OnEnable()
    {
        EventBus.OnRoundEnded += OnRoundEnded;
    }

    private void OnDisable()
    {
        EventBus.OnRoundEnded -= OnRoundEnded;
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void Init()
    {
        InitMaps();

        _photonView = GetComponent<PhotonView>();
        _photonView.ViewID = 0;
        _photonView.ViewID = 666;

        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        _playersNicknames = new string[maxPlayers];
        _playersCupsCount = new int[maxPlayers];
        _playersSkinID = new int[maxPlayers];

        GameData game = new();
        game.CallMethod<MatchHandler>(nameof(OnPlayerConnect), _photonView, RpcTarget.All, PlayerData.GetNickname(), PlayerData.GetSkinID());
    }

    private void InitMaps()
    {
        int scenesCountBeforeMaps = 4;
        int mapsCount = (int)GameSettings.Scene.Fallground + 1;
        _allMaps = new int[mapsCount - scenesCountBeforeMaps];

        int mapAdded = 0;
        for (int i = (int)GameSettings.Scene.Street; i < mapsCount; i++)
        {
            _allMaps[mapAdded++] = i;
        }
        _availableMaps = _allMaps.ToList();

        int randomMap = Random.Range(0, _availableMaps.Count);
        NextMap = _availableMaps[randomMap];
        _availableMaps.RemoveAt(randomMap);
    }

    public void CreateBotData(int ID, int nameID, int skinID)
    {
        GameData game = new();
        game.CallMethod<MatchHandler>(nameof(CreateBotForAll), _photonView, RpcTarget.All, ID, nameID, skinID);
    }

    [PunRPC]
    private void CreateBotForAll(int ID, int nameID, int skinID)
    {
        StringBus stringBus = new();
        _playersNicknames[ID] = stringBus.NicknameBus[nameID];
        _playersSkinID[ID] = skinID;
        _playersCupsCount[ID] = 0;
    }

    [PunRPC]
    private void OnPlayerConnect(string nickname, int skinID)
    {
        _playersNicknames[PlayersCount] = nickname;
        _playersSkinID[PlayersCount] = skinID;
        _playersCupsCount[PlayersCount] = 0;
    }

    public void SetIDForCharacter(Character character)
    {
        if (character == null) return;
        character.SetPlayerID(PlayersCount);
        if(character.IsABot == false && character.PhotonView.IsMine)
        {
            _currentPlayerID = PlayersCount;
        }
        PlayersCount++;
    }

    public void GiveWinnerCoup(int playerID, bool isABot)
    {
        GameData game = new();
        game.CallMethod<MatchHandler>(nameof(GiveCoup), _photonView, RpcTarget.All, playerID, isABot);
    }

    [PunRPC]
    private void GiveCoup(int playerID, bool isABot)
    {
        if (++_playersCupsCount[playerID] >= 3)
        {
            if (isABot == false && _currentPlayerID == playerID)
            {
                PlayerData.GiveWinnerAward();
                EventBus.OnPlayerWin?.Invoke();
                SaveData.Instance.Stats(SaveData.Statistics.MatchEnd);
            }
            else
            {
                PlayerData.GiveExp(50);

                if (PlayerData.GetDroppedPlayersCount > 0)
                {
                    Coins.Give(CoinsHandler.GiveReason.ForKiller);
                }

                SaveData.Instance.Stats(SaveData.Statistics.MatchEnd);
            }
        }
    }

    public void SetNickname(int playerID, string nickname)
    {
        _playersNicknames[playerID] = nickname;
    }

    public void ResetData()
    {
        _playersNicknames = null;
        _playersCupsCount = null;
        _playersSkinID = null;
        PlayersCount = 0;
        CurrentRound = 0;

        Destroy(gameObject);
    }

    private void OnRoundEnded(int winnerPhotonID)
    {
        int randomMap = Random.Range(0, _availableMaps.Count);
        NextMap = _availableMaps[randomMap];
        _availableMaps.RemoveAt(randomMap);
        if(_availableMaps.Count == 0)
        {
            _availableMaps = _allMaps.ToList();
        }

        CurrentRound++;
    }

    public void StartNextRound()
    {
        _photonView.RPC(nameof(LoadLevelProgressForAll), RpcTarget.All);
    }

    [PunRPC]
    private void LoadLevelProgressForAll()
    {
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(NextMap);
        LoadingUI.Show(LoadingShower.Type.Progress);

        while (PhotonNetwork.LevelLoadingProgress < 1f)
        {
            LoadingUI.UpdateProgress(PhotonNetwork.LevelLoadingProgress);
            yield return new WaitForEndOfFrame();
        }
    }
}
