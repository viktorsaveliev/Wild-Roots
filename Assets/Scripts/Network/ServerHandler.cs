using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(WeaponSpawner))]
public class ServerHandler : MonoBehaviourPunCallbacks
{
    public List<Character> Characters { get; private set; } = new();

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private Transform[] _spawnPositions;

    [SerializeField] private JoystickMovement _joystickMove;
    [SerializeField] private JoystickAttack _joystickAttack;
    [SerializeField] private Text _roundText;

    public int CurrentRound;
    private WeaponSpawner _weapons;
    public PhotonView ServerPhotonView;

    private void Awake()
    {
        Notice.HideDialog();
        Notice.HideSimple();

        DOTween.SetTweensCapacity(100, 10);
    }

    /*private IEnumerator SetOfflineMode()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        PhotonNetwork.OfflineMode = true;
    }*/

    private void Start()
    {
        ServerPhotonView = GetComponent<PhotonView>();
        _weapons = GetComponent<WeaponSpawner>();

        /*if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            GameSettings.OfflineMode = true;
            yield return StartCoroutine(SetOfflineMode());
        }*/

        if (PhotonNetwork.OfflineMode == false && PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.GetPing() > 150 && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer.GetNext());
                print("[WRS]: Changed Master Client. Reason: high ping");
            }
        }

        SpawnPlayer();

        if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
        {
            _weapons.CreateWeapons();
            StartCoroutine(CreateBots());
        }

        SaveData.Instance.Stats(SaveData.Statistics.MatchStart);
    }

    private void SpawnPlayer()
    {
        GameObject PlayerCharacter;
        if (PhotonNetwork.OfflineMode)
        {
            PlayerCharacter = Instantiate(_playerPrefab, new Vector3(0, 5, 0), Quaternion.identity);
        }
        else
        {
            PlayerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0, 5, 0), Quaternion.identity);
        }

        Character player = PlayerCharacter.GetComponent<Character>();

        _joystickMove.Init(player);
        _joystickAttack.Init(player);

        if (PhotonNetwork.OfflineMode)
        {
            AddCharacterInList(player);
        }
        else
        {
            ServerPhotonView.RPC(nameof(AddCharacterInList), RpcTarget.All, player.PhotonView.ViewID);
        }

        PhotonNetwork.LocalPlayer.TagObject = gameObject;

        int spawnPoint = PhotonNetwork.OfflineMode ? 0 : PhotonNetwork.LocalPlayer.ActorNumber - 1;
        PlayerCharacter.transform.position = _spawnPositions[spawnPoint].position;
        
        player.Nickname = PlayerData.GetNickname();
        if (player.Nickname == string.Empty)
        {
            StringBus stringBus = new();
            string nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];
            player.Nickname = nickname;
            PlayerData.SetNickname(nickname);
        }

        if(Match.CurrentRound == 0)
        {
            Match.SetIDForCharacter(player);
        }
        else
        {
            player.SetPlayerID(Characters.Count-1);
        }
       
        if (PhotonNetwork.OfflineMode)
        {
            player.Skin.Change(PlayerData.GetSkinID());
            SetCharacterNickname(player, player.Nickname);
        }
        else
        {
            player.Skin.UpdateForAll();
            ServerPhotonView.RPC(nameof(SetCharacterNickname), RpcTarget.Others, player.PhotonView.ViewID, player.Nickname);
        }

        PlayerData.SaveRoundStartTime(Time.time);
    }

    private IEnumerator CreateBots()
    {
        int playerCount = PhotonNetwork.OfflineMode ? 1 : PhotonNetwork.CurrentRoom.PlayerCount;
        int MaxPlayers = PhotonNetwork.OfflineMode ? 3 : PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerCount < MaxPlayers)
        {
            if (ServerPhotonView == null)
            {
                ServerPhotonView = GetComponent<PhotonView>();
            }

            StringBus stringBus = new();
            for (int i = playerCount; i < MaxPlayers; i++)
            {
                GameObject charObject;
                if (PhotonNetwork.OfflineMode)
                {
                    yield return charObject = Instantiate(_botPrefab, _spawnPositions[i].position, Quaternion.identity);
                }
                else
                {
                    yield return charObject = PhotonNetwork.InstantiateRoomObject(_botPrefab.name, _spawnPositions[i].position, Quaternion.identity);
                }

                Character character = charObject.GetComponent<Character>();

                if(Match.CurrentRound == 0)
                {
                    Match.SetIDForCharacter(character);

                    GameData game = new();
                    int nicknameID = Random.Range(0, stringBus.NicknameBus.Length);
                    int skinID = Random.Range(1, game.SkinsCount);

                    Match.CreateBotData(character.PlayerID, nicknameID, skinID);
                }
                else
                {
                    character.SetPlayerID(i);
                }

                character.Nickname = Match.GetPlayerNickname(character.PlayerID);
                character.Skin.Change(Match.GetPlayersSkinID(character.PlayerID), true);

                if(PhotonNetwork.OfflineMode)
                {
                    AddCharacterInList(character);
                    SetCharacterNickname(character, character.Nickname);
                }
                else
                {
                    ServerPhotonView.RPC(nameof(AddCharacterInList), RpcTarget.All, character.PhotonView.ViewID);
                    ServerPhotonView.RPC(nameof(SetCharacterNickname), RpcTarget.Others, character.PhotonView.ViewID, character.Nickname);
                }
            }
        }
    }

    private IEnumerator CreateBot(string nickname, int skinID, Vector3 position)
    {
        GameObject charObject;
        yield return charObject = PhotonNetwork.InstantiateRoomObject(_botPrefab.name, position, Quaternion.identity);

        Character character = charObject.GetComponent<Character>();
        ServerPhotonView.RPC(nameof(AddCharacterInList), RpcTarget.All, character.PhotonView.ViewID);
        character.Nickname = nickname;

        character.Skin.Change(skinID);
        ServerPhotonView.RPC(nameof(SetCharacterNickname), RpcTarget.Others, character.PhotonView.ViewID, character.Nickname);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false && PhotonNetwork.OfflineMode == false)
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
        if (GameSettings.OfflineMode) return;

        ResetPlayerData();
        SceneManager.LoadScene((int)GameSettings.Scene.Lobby);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (GameSettings.OfflineMode) return;

        Character player = Characters.FirstOrDefault(p => p.PhotonView.CreatorActorNr == otherPlayer.ActorNumber);
        if(player != null)
        {
            player.Weapon.DeleteWeapon(true);
            Characters.Remove(player);

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(CreateBot(player.Nickname, player.Skin.GetSkinID, player.transform.position));
                //EventBus.OnCharacterLose?.Invoke(player.PhotonView.ViewID);
            }

            Destroy(player.gameObject);
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
        SaveData.Instance.Stats(SaveData.Statistics.LeftTheMatch);
    }
}