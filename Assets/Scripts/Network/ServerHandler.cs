using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(WeaponsHandler), typeof(HoneycombHandler))]
public class ServerHandler : MonoBehaviourPunCallbacks
{
    private const int MAX_PLAYERS = 5;
    public List<Character> Characters { get; private set; } = new();

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private Transform[] _spawnPositions;

    [SerializeField] private JoystickMovement _joystickMove;
    [SerializeField] private JoystickAttack _joystickAttack;
    [SerializeField] private Text _roundText;

    private HoneycombHandler _honeycombHandler;

    public int CurrentRound;
    //private int _timeToNextRound;

    //private Coroutine _timer;
    [SerializeField] private Text _textTime;
    private WeaponsHandler _weapons;
    public PhotonView ServerPhotonView;

    private void Awake()
    {
        DOTween.SetTweensCapacity(500, 50);
        /*if(PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            StartCoroutine(SetOfflineMode());
        }*/

        if (PhotonNetwork.IsMasterClient)
        {
            if(PhotonNetwork.GetPing() > 150 && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer.GetNext());
                print("[WRS]: Changed Master Client. Reason: high ping");
            }
        }
    }

    /*private IEnumerator SetOfflineMode()
    {
        _isOfflineMode = true;
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        PhotonNetwork.OfflineMode = true;
    }*/

    private void Start()
    {
        GameObject PlayerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0, 5, 0), Quaternion.identity);
        Character player = PlayerCharacter.GetComponent<Character>();

        _joystickMove.Init(player);
        _joystickAttack.Init(player);

        _honeycombHandler = GetComponent<HoneycombHandler>();
        _honeycombHandler.Init();

        _weapons = GetComponent<WeaponsHandler>();
        _weapons.IsNeedUpdateHoneycomb = true;

        ServerPhotonView = GetComponent<PhotonView>();
        ServerPhotonView.RPC(nameof(AddCharacterInList), RpcTarget.All, player.PhotonView.ViewID);

        PhotonNetwork.LocalPlayer.TagObject = gameObject;
        PlayerCharacter.transform.position = _spawnPositions[PhotonNetwork.LocalPlayer.ActorNumber-1].position;

        player.Nickname = PhotonNetwork.LocalPlayer.NickName;
        player.Skin.UpdateForAll();

        /*StringBus stringBus = new();
        PlayerPrefs.DeleteKey(stringBus.SkinID);
        PlayerPrefs.Save();*/

        ServerPhotonView.RPC(nameof(SetCharacterNickname), RpcTarget.Others, player.PhotonView.ViewID, player.Nickname);
  
        if (PhotonNetwork.IsMasterClient)
        {
            _weapons.CreateWeapons(CurrentRound+1);
            Invoke(nameof(CreateBots), 1f);
        }
    }

    private void CreateBots()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < MAX_PLAYERS)
        {
            if (ServerPhotonView == null)
            {
                ServerPhotonView = GetComponent<PhotonView>();
            }

            StringBus stringBus = new();
            for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < MAX_PLAYERS; i++)
            {
                Character character = PhotonNetwork.InstantiateRoomObject(_botPrefab.name, _spawnPositions[i].position, Quaternion.identity).GetComponent<Character>();
                ServerPhotonView.RPC(nameof(AddCharacterInList), RpcTarget.All, character.PhotonView.ViewID);
                character.Nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];

                ServerPhotonView.RPC(nameof(SetCharacterNickname), RpcTarget.Others, character.PhotonView.ViewID, character.Nickname);
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer.GetNext());
                print("[WRS]: Changed Master Client. Reason: turned the game");
            }
        }
    }

    #region Network

    [PunRPC]
    public void AddCharacterInList(int viewID)
    {
        Character player = PhotonView.Find(viewID).GetComponent<Character>();
        Characters.Add(player);
    }

    public void AddCharacterInList(Character player)
    {
        Characters.Add(player);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        _weapons.IsNeedUpdateHoneycomb = true;
        if (PhotonNetwork.IsMasterClient)
        {
            _weapons.StartTimerForWeaponSpawn();

            foreach (Character character in Characters)
            {
                if (character.gameObject.activeSelf == false) continue;
                if (character.TryGetComponent(out CharacterAI ai))
                {
                    ai.StartInfinityTimer();
                }
            }
        }
    }

    public override void OnLeftRoom()
    {
        ResetPlayerData();
        SceneManager.LoadScene(0); // lobby
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Character player = Characters.FirstOrDefault(p => p.PhotonView.CreatorActorNr == otherPlayer.ActorNumber);
        if(player != null)
        {
            player.Weapon.DeleteWeapon(true);
            Characters.Remove(player);
            Destroy(player.gameObject, 1f);

            if (PhotonNetwork.IsMasterClient)
            {
                EventBus.OnCharacterLose?.Invoke(player.PhotonView.ViewID);
            }
        }
    }

    #endregion

    [PunRPC]
    public void SetCharacterNickname(int viewID, string nickname)
    {
        Character character = PhotonView.Find(viewID).GetComponent<Character>();
        character.Nickname = nickname;
    }

    public void SetCharacterNickname(Character character, string nickname)
    {
        character.Nickname = nickname;
    }

    private void ResetPlayerData()
    {
        DOTween.Clear();
    }

    private void OnApplicationQuit()
    {
        ResetPlayerData();
    }

    /*public void SetTimeToNextRound(int sec)
    {
        if (_timer != null) StopCoroutine(_timer);
        _timeToNextRound = sec;
        _textTime.text = _timeToNextRound.ToString();
        _timer = StartCoroutine(SecTimer());
    }

    private IEnumerator SecTimer()
    {
        while (_timeToNextRound > 0)
        {
            yield return new WaitForSeconds(1f);
            if (--_timeToNextRound == 5)
            {
                _textTime.enabled = true;
                if (PhotonNetwork.IsMasterClient)
                {
                    ServerPhotonView.RPC("DestroyHoneycombs", RpcTarget.All, CurrentRound);
                }
            }
            _textTime.text = $"{_timeToNextRound}"; //_timeToNextRound > 9 ? ($"00:{_timeToNextRound}") : ($"00:0{_timeToNextRound}");
        }
        if (_timeToNextRound <= 0)
        {
            _textTime.enabled = false;
            if (CurrentRound < 3)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    _weaponSpawner.DeleteWeaponInRoundLayer(CurrentRound);
                }
                CurrentRound++;
                SetTimeToNextRound(30);
                _roundText.text = $"Round: {CurrentRound+1}/4";
            }
            else
            {
                _roundText.text = "Final Round";
            }
        }
    }*/
}
