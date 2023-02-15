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

    [SerializeField] private Sprite _fallIcon;

    private bool _killLogIsActive;
    private Coroutine _lifetimeKillLog;

    private PhotonView _photonView;
    private ServerHandler _serverHandler;

    private void Start()
    {
        _serverHandler = GetComponent<ServerHandler>();
        _photonView = PhotonView.Get(this);
        _playersCount.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}";
    }

    private void OnEnable()
    {
        EventBus.OnPlayerFall += UpdateHealthUI;
        EventBus.OnPlayerLose += MasterUpdatePlayers;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerFall -= UpdateHealthUI;
        EventBus.OnPlayerLose -= MasterUpdatePlayers;
    }

    public void MasterUpdatePlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int playersCount = 0;
            PlayerInfo winner = null;
            foreach (PlayerInfo player in _serverHandler.Players)
            {
                if (player.Health && player.Health.Health > 0)
                {
                    playersCount++;
                    winner = player;
                }
            }
            if(playersCount < 2)
            {
                Invoke(nameof(MatchEnded), 7f);
            }
            _photonView.RPC(nameof(UpdatePlayersCount), RpcTarget.All, playersCount, 
                winner != null ? winner.PhotonView.ViewID : -1);
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
        _playersCount.text = $"{count}";
        if (count < 2 && winnerID != -1) EventBus.OnMatchEnded?.Invoke(winnerID);
    }

    private void UpdateHealthUI(PlayerInfo player, int health)
    {
        if (player.PhotonView.IsMine)
        {
            _heartsUI[health].transform.DOShakeRotation(1f, 50, 10, 90).OnComplete(() =>
            {
                _heartsUI[health].DOColor(new Color(1, 1, 1, 0.5f), 1f);
                _heartsUI[health].transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
            });
            _photonView.RPC("EnableKillLogUI", RpcTarget.All, player.PhotonView.ViewID, health);
        }
    }

    [PunRPC]
    public void EnableKillLogUI(int ViewID, int health)
    {
        PlayerInfo killed = PhotonView.Find(ViewID).GetComponent<PlayerInfo>();
        if(!_killLogIsActive)
        {
            if(killed.Health.FromWhomDamage != null)
            {
                _killer.text = $"Player {killed.Health.FromWhomDamage.Player.GetPhotonView.ViewID}";
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

    private void DisableKillLogUI(PlayerInfo player = null, int health = 0, bool restart = false)
    {
        _killLog.rectTransform.DOAnchorPosX(210f, 0.4f).OnComplete(() =>
        {
            _killLogIsActive = false;
            _killLog.gameObject.SetActive(false);
            if (restart) EnableKillLogUI(player.GetComponent<PhotonView>().ViewID, health);
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