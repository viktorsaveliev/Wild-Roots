using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;

public class CharacterHUD : MonoBehaviour
{
    [SerializeField] private GameObject _anotherPlayer;
    [SerializeField] private GameObject _minePlayer;

    [SerializeField] private Character _character;

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
        EventBus.OnCharacterTakeDamage += UpdateHUD;
        EventBus.OnCharacterTakeAim += ShowAimIndicator;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterTakeDamage -= UpdateHUD;
        EventBus.OnCharacterTakeAim -= ShowAimIndicator;
    }

    private void Start()
    {
        Camera camera = Camera.main;
        _canvas.worldCamera = camera;
        _target = camera.transform;

        _photonView = PhotonView.Get(_character);
        if (_photonView.IsMine && _character.IsABot == false)
        {
            _minePlayer.SetActive(true);
        }
        else
        {
            _nickname.text = $"Player {_photonView.ViewID}";
            _anotherPlayer.SetActive(true);
        }

        _character.HUD = this;
    }

    private void Update()
    {
        transform.LookAt(_target);
        //_canvas.transform.rotation = _target.transform.rotation;
    }

    private void UpdateHUD(Character character)
    {
        if(character == _character)
        {
            if (!_photonView.IsMine || character.IsABot)
            {
                if(_damageIndicator != null) _damageIndicator.color = _damageGradient.Evaluate(_character.Health.DamageStrength);
            }
            else
            {
                if(_iconForMinePlayer != null) _iconForMinePlayer.color = _damageGradient.Evaluate(_character.Health.DamageStrength);
            }
        }
    }

    private void ShowAimIndicator(Character character)
    {
        if (character != _character || _aimIndicator == null) return;
        _aimIndicator.enabled = true;

        if (_aimIndicatorAnimation != null) DOTween.Kill(_aimIndicatorAnimation);
        _aimIndicatorAnimation = _aimIndicator.transform.DOShakePosition(0.5f).SetLoops(-1);
    }

    public void HideAimIndicator()
    {
        if(_aimIndicator == null) return;
        _aimIndicator.enabled = false;
        if (_aimIndicatorAnimation != null)
        {
            DOTween.Kill(_aimIndicatorAnimation);
            _aimIndicatorAnimation = null;
        }
    }
}
