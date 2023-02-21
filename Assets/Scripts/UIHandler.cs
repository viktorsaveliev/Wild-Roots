using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using System.Collections;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private Image[] _heartsUI;

    [SerializeField] private Image _killLog;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private Text _killer;
    [SerializeField] private Text _killed;
    [SerializeField] private Text _playersCount;
    [SerializeField] private Text _playersText;

    [SerializeField] private Sprite _fallIcon;

    [SerializeField] private Button _buttonBackToLobby;

    private bool _killLogIsActive;
    private Coroutine _lifetimeKillLog;

    private PhotonView _photonView;
    private ServerHandler _serverHandler;

    private void Start()
    {
        _serverHandler = GetComponent<ServerHandler>();
        _photonView = PhotonView.Get(this);

        if(GameSettings.GameMode == GameModeSelector.GameMode.PvP)
        {
            _playersCount.text = $"{_serverHandler.Characters.Count}";
            _buttonBackToLobby.gameObject.SetActive(false);
        }
        else
        {
            _playersCount.gameObject.SetActive(false);
            _playersText.gameObject.SetActive(false);
            _buttonBackToLobby.gameObject.SetActive(true);

            for(int i = 0; i < _heartsUI.Length; i++)
            {
                _heartsUI[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        EventBus.OnCharacterFall += UpdateHealthUI;
        EventBus.OnCharacterLose += MasterUpdatePlayers;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterFall -= UpdateHealthUI;
        EventBus.OnCharacterLose -= MasterUpdatePlayers;
    }

    public void MasterUpdatePlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int playersCount = 0;
            Character winner = null;
            foreach (Character player in _serverHandler.Characters)
            {
                if (player.Health && player.Health.Health > 0)
                {
                    playersCount++;
                    winner = player;
                }
            }
            if(GameSettings.GameMode == GameModeSelector.GameMode.PvP)
            {
                if (playersCount < 2)
                {
                    Invoke(nameof(MatchEnded), 7f);
                }
                _photonView.RPC(nameof(UpdatePlayersCount), RpcTarget.All, playersCount,
                    winner != null ? winner.PhotonView.ViewID : -1);
            }
        }
    }

    private void MatchEnded()
    {
        DOTween.Clear();
        PhotonNetwork.LoadLevel(0);
        _photonView.RPC(nameof(LeaveRoom), RpcTarget.All);
    }

    [PunRPC]
    public void LeaveRoom()
    {
        if(PhotonNetwork.IsConnectedAndReady) PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    public void UpdatePlayersCount(int count, int winnerID = -1)
    {
        if(_playersCount != null) _playersCount.text = $"{count}";
        if (count < 2 && winnerID != -1)
        {
            EventBus.OnMatchEnded?.Invoke(winnerID);
        }
    }

    private void UpdateHealthUI(Character character, int health)
    {
        if (GameSettings.GameMode == GameModeSelector.GameMode.PvP && character.IsABot == false)
        {
            if (character.PhotonView.IsMine && _heartsUI[health] != null)
            {
                _heartsUI[health].transform.DOShakeRotation(1f, 50, 10, 90).OnComplete(() =>
                {
                    _heartsUI[health].DOColor(new Color(1, 1, 1, 0.5f), 1f);
                    _heartsUI[health].transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
                });
            }
        }
        _photonView.RPC(nameof(EnableKillLogUI), RpcTarget.All, character.PhotonView.ViewID, health);
    }

    [PunRPC]
    public void EnableKillLogUI(int ViewID, int health)
    {
        if (_killer == null || _killed == null || _killLog == null || _weaponIcon == null || _fallIcon == null) return;

        Character killed = PhotonView.Find(ViewID).GetComponent<Character>();
        if(!_killLogIsActive)
        {
            if(killed.Health.FromWhomDamage != null)
            {
                _killer.text = $"Player {killed.Health.FromWhomDamage.CharacterOwner.GetPhotonView().ViewID}";
                _killed.text = $"Player {ViewID}";
                _weaponIcon.sprite = killed.Health.FromWhomDamage.GetSpriteIcon;
            }
            else
            {
                _killer.text = _killed.text = $"Player {ViewID}";
                _weaponIcon.sprite = _fallIcon;
            }
            _killLog.gameObject.SetActive(true);
            _killLog.rectTransform.DOAnchorPosX(-207.8f, 0.4f);
            _killLogIsActive = true;

            if (_lifetimeKillLog != null) StopCoroutine(_lifetimeKillLog);
            _lifetimeKillLog = StartCoroutine(KillLogTimer());
        }
        else
        {
            DisableKillLogUI(killed, health, true);
        }
    }

    private void DisableKillLogUI(Character character = null, int health = 0, bool restart = false)
    {
        if (_killLog == null) return;
        _killLog.rectTransform.DOAnchorPosX(210f, 0.4f).OnComplete(() =>
        {
            _killLogIsActive = false;
            _killLog.gameObject.SetActive(false);
            if (restart) EnableKillLogUI(character.GetComponent<PhotonView>().ViewID, health);
        });
    }

    private IEnumerator KillLogTimer()
    {
        yield return new WaitForSeconds(3f);
        if(_killLogIsActive)
        {
            DisableKillLogUI();
        }
    }
}
