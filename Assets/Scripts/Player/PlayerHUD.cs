using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject _anotherPlayer;
    [SerializeField] private GameObject _minePlayer;

    [SerializeField] private PlayerInfo _player;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private Text _nickname;
    [SerializeField] private Image _damageIndicator;
    [SerializeField] private Image _iconForMinePlayer;
    [SerializeField] private Gradient _damageGradient;

    [SerializeField] private Image _aimIndicator;
    private Tweener _aimIndicatorAnimation;

    private Transform _target;
    private PhotonView _photonView;

    private void OnEnable()
    {
        EventBus.OnPlayerTakeDamage += UpdateHUD;
        EventBus.OnPlayerTakeAim += ShowAimIndicator;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerTakeDamage -= UpdateHUD;
        EventBus.OnPlayerTakeAim -= ShowAimIndicator;
    }

    private void Start()
    {
        Camera camera = Camera.main;
        _canvas.worldCamera = camera;
        _target = camera.transform;

        _photonView = PhotonView.Get(_player);
        if (_photonView.IsMine)
        {
            _minePlayer.SetActive(true);
        }
        else
        {
            _nickname.text = $"Player {_photonView.ViewID}";
            _anotherPlayer.SetActive(true);
        }

        _player.HUD = this;
    }

    private void Update()
    {
        transform.LookAt(_target);
        //_canvas.transform.rotation = _target.transform.rotation;
    }

    private void UpdateHUD(PlayerInfo player)
    {
        if(player == _player)
        {
            if (!_photonView.IsMine)
            {
                _damageIndicator.color = _damageGradient.Evaluate(_player.Health.DamageStrength);
            }
            else
            {
                _iconForMinePlayer.color = _damageGradient.Evaluate(_player.Health.DamageStrength);
            }
        }
    }

    private void ShowAimIndicator(PlayerInfo player)
    {
        if (player != _player) return;
        _aimIndicator.enabled = true;

        if (_aimIndicatorAnimation != null) DOTween.Kill(_aimIndicatorAnimation);
        _aimIndicatorAnimation = _aimIndicator.transform.DOShakePosition(0.5f).SetLoops(-1);
    }

    public void HideAimIndicator()
    {
        _aimIndicator.enabled = false;
        if (_aimIndicatorAnimation != null)
        {
            DOTween.Kill(_aimIndicatorAnimation);
            _aimIndicatorAnimation = null;
        }
    }
}
