using UnityEngine;

public class JoystickMovement : JoystickHandler
{
    private PlayerInfo _player;
    private bool _isInit = false;

    public void Init(PlayerInfo playerControl)
    {
        _player = playerControl;
        _isInit = true;

        StringBus stringBus = new();
        int playerDevice = PlayerPrefs.GetInt(stringBus.PlayerDevice);
        if (playerDevice == 1) gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if(_isInit) OnPointerUp(null);
    }

    private void Update()
    {
        if (!_isInit || !_player.PhotonView.IsMine || !_player.Move.GetMoveActive()) return;
        if (InputVector.x != 0 || InputVector.y != 0)
        {
            _player.Move.Move(new Vector3(InputVector.x, 0, InputVector.y));
            if (!_player.Move.IsTakesAim && _player.Move.IsCanRotate) _player.Move.Rotate(new Vector3(InputVector.x, 0, InputVector.y));
        }
    }

    protected override void OnPlayerMouseUp()
    {
        if (_player.Animator == null) return;
        StringBus stringBus = new();
        _player.Animator.SetFloat(stringBus.AnimationSpeed, 0);
    }
}
