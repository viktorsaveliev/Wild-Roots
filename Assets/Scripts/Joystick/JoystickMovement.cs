using UnityEngine;

public class JoystickMovement : JoystickHandler
{
    private Character _character;
    private bool _isInit = false;

    public void Init(Character character)
    {
        _character = character;
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
        if (!_isInit || !_character.PhotonView.IsMine || !_character.Move.GetMoveActive()) return;
        if (InputVector.x != 0 || InputVector.y != 0)
        {
            _character.Move.Move(new Vector3(InputVector.x, 0, InputVector.y));
            if (!_character.Move.IsTakesAim && _character.Move.IsCanRotate) _character.Move.Rotate(new Vector3(InputVector.x, 0, InputVector.y));
        }
    }

    protected override void OnPlayerMouseUp()
    {
        if (_character.Animator == null) return;
        StringBus stringBus = new();
        _character.Animator.SetFloat(stringBus.AnimationSpeed, 0);
    }
}
