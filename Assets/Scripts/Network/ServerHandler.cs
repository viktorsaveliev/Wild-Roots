using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(WeaponsHandler), typeof(HoneycombHandler))]
public class ServerHandler : MonoBehaviourPunCallbacks
{
    public List<PlayerInfo> Players { get; private set; } = new();

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private JoystickMovement _joystickMove;
    [SerializeField] private JoystickAttack _joystickAttack;
    [SerializeField] private Text _roundText;

    private HoneycombHandler _honeycombHandler;

    public int CurrentRound;
    private int _timeToNextRound;

    private Coroutine _timer;
    [SerializeField] private Text _textTime;
    private WeaponsHandler _weaponSpawner;
    public PhotonView ServerPhotonView;

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(PhotonNetwork.GetPing() > 100 && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer.GetNext());
                print("[WRS]: Changed Master Client. Reason: high ping");
            }
        }
    }

    private void Start()
    {
        GameObject PlayerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0, 5, 0), Quaternion.identity);
        PlayerInfo playerControl = PlayerCharacter.GetComponent<PlayerInfo>();

        _joystickMove.Init(playerControl);
        _joystickAttack.Init(playerControl);
        //Camera.main.GetComponent<CameraMoveToPlayer>().Player = playerControl;

        _honeycombHandler = GetComponent<HoneycombHandler>();
        _honeycombHandler.Init();

        _weaponSpawner = GetComponent<WeaponsHandler>();
        _weaponSpawner.IsNeedUpdateHoneycomb = true;

        if (PhotonNetwork.IsMasterClient)
        {
            _weaponSpawner.CreateWeapons(CurrentRound); // _honeycombHandler.GetHoneycombCircles[1]
        }

        ServerPhotonView = PhotonView.Get(this);
        ServerPhotonView.RPC(nameof(AddPlayerInList), RpcTarget.All, playerControl.PhotonView.ViewID);
        PhotonNetwork.LocalPlayer.TagObject = gameObject;
    }

    #region Network

    [PunRPC]
    public void AddPlayerInList(int viewID)
    {
        PlayerInfo player = PhotonView.Find(viewID).GetComponent<PlayerInfo>();
        Players.Add(player);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        GameObject master = (GameObject)newMasterClient.TagObject;
        //_honeycombHandler = master.GetComponent<HoneycombHandler>();
        _honeycombHandler.ReInit(_timeToNextRound);

        WeaponsHandler weaponSpawner = master.GetComponent<WeaponsHandler>();
        weaponSpawner.IsNeedUpdateHoneycomb = true;
        weaponSpawner.StartTimerForWeaponSpawn();

        if(GameSettings.GameMode == SelectGameMode.GameMode.Deathmatch)
        {
            weaponSpawner.Pool.Clear();
            Weapon[] weapons = FindObjectsOfType<Weapon>();
            foreach(Weapon weapon in weapons)
            {
                weaponSpawner.Pool.Add(weapon.gameObject);
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
        PlayerInfo player = Players.First(p => p.PhotonView.CreatorActorNr == otherPlayer.ActorNumber);
        if(player != null)
        {
            player.Weapon.DeleteWeapon(true);
            Players.Remove(player);
            Destroy(player.gameObject, 1f);

            if (PhotonNetwork.IsMasterClient && GameSettings.GameMode == SelectGameMode.GameMode.PvP)
            {
                EventBus.OnPlayerLose?.Invoke();
            }
        }
    }

    #endregion

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
