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
    [SerializeField] private RoundEndUI _roundEnd;

    private bool _killLogIsActive;
    private Coroutine _lifetimeKillLog;

    private PhotonView _photonView;
    private ServerHandler _serverHandler;
    private string _winnerNickname;

    private void Start()
    {
        _serverHandler = GetComponent<ServerHandler>();
        _photonView = PhotonView.Get(this);

        Invoke(nameof(UpdatePlayersCount), 2f);
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

    private void MasterUpdatePlayers(int loserViewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int playersCount = 0;
            Character winner = null;
            foreach (Character player in _serverHandler.Characters)
            {
                if (player.Health && player.Health.Value > 0)
                {
                    playersCount++;
                    winner = player;
                }
            }
            if (playersCount < 2)
            {
                if (winner != null) _winnerNickname = winner.Nickname;
                _photonView.RPC(nameof(GoToNextRound), RpcTarget.All);
                Invoke(nameof(RoundEnded), 3f);
            }
            _photonView.RPC(nameof(UpdatePlayersCount), RpcTarget.All, playersCount,
                winner != null ? winner.PhotonView.ViewID : -1);
        }
    }

    private void RoundEnded()
    {
        PhotonNetwork.LoadLevel((int)GameSettings.Scene.MatchInformer);
    }

    [PunRPC]
    private void GoToNextRound()
    {
        if (_winnerNickname != string.Empty) _roundEnd.Show(_winnerNickname);
    }

    private void UpdatePlayersCount()
    {
        _playersCount.text = $"{_serverHandler.Characters.Count}";
    }

    [PunRPC]
    private void UpdatePlayersCount(int count, int winnerID = -1)
    {
        if(_playersCount != null) _playersCount.text = $"{count}";
        if (count < 2 && winnerID != -1)
        {
            EventBus.OnRoundEnded?.Invoke(winnerID);
        }
    }

    private void UpdateHealthUI(Character character, int health)
    {
        if (character.IsABot == false)
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
        if(PhotonNetwork.IsMasterClient)
        {
            int killerViewID = -1;
            if(character.Health.FromWhomDamage != -1) killerViewID = character.Health.FromWhomDamage;
            _photonView.RPC(nameof(EnableKillLogUI), RpcTarget.All, character.PhotonView.ViewID, killerViewID, health);
        }
    }

    [PunRPC]
    private void EnableKillLogUI(int ViewID, int killerViewID, int health)
    {
        if (_killer == null || _killed == null || _killLog == null || _weaponIcon == null || _fallIcon == null) return;

        Character killed = PhotonView.Find(ViewID).GetComponent<Character>();
        Character killer = null;

        if (killerViewID != -1) killer = PhotonView.Find(killerViewID).GetComponent<Character>();

        if (!_killLogIsActive)
        {
            if(killerViewID != -1)
            {
                _killer.text = $"{killer.Nickname}";
                _killed.text = $"{killed.Nickname}";
                //_weaponIcon.sprite = killed.Health.FromWhomDamage.GetSpriteIcon;
            }
            else
            {
                _killer.text = _killed.text = $"{killed.Nickname}";
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
        if (_killLog == null || _killLogIsActive == false) return;
        _killLog.rectTransform.DOAnchorPosX(210f, 0.4f).OnComplete(() =>
        {
            _killLogIsActive = false;
            _killLog.gameObject.SetActive(false);
            if (restart)
            {
                int killerViewID = -1;
                if(character.Health.FromWhomDamage != -1) killerViewID = character.Health.FromWhomDamage;
                EnableKillLogUI(character.GetComponent<PhotonView>().ViewID, killerViewID, health);
            }
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