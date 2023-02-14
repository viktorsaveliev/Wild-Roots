using Photon.Pun;
using UnityEngine;
using System.Collections;

public class JoystickAttack : JoystickHandler
{
    [SerializeField] private GameObject _tutorialHand;

    private TrajectoryRenderer _trajectoryRenderer;
    private Coroutine _updateTrajectory;

    private PlayerInfo _player;
    private bool _isInit = false;

    protected override void OnPlayerMouseDrag()
    {
        if (!_player.gameObject.activeSelf || !_isInit || !_player.PhotonView.IsMine || !_player.Move.GetMoveActive) return;
        if (InputVector.x != 0 || InputVector.y != 0)
        {
            _player.Move.RotateCharacter(new Vector3(InputVector.x, 0, InputVector.y));
        }
    }

    private IEnumerator UpdateTrajectory()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Vector3 playerPos = new(_player.transform.position.x, _player.transform.position.y + 0.5f, _player.transform.position.z);
            _trajectoryRenderer.ShowTrajectory(playerPos, new Vector3(InputVector.x, 0.5f, InputVector.y) * 8);
        }
    }

    public void Init(PlayerInfo playerControl)
    {
        _player = playerControl;
        _trajectoryRenderer = GetComponent<TrajectoryRenderer>();
        _isInit = true;
    }

    protected override void OnPlayerMouseUp()
    {
        if (!_player.gameObject.activeSelf) return;

        if (_updateTrajectory != null)
        {
            StopCoroutine(_updateTrajectory);
            _updateTrajectory = null;
        }

        _player.Move.IsTakesAim = false;
        _player.Move.SetDelayToRotate();

        StringBus stringBus = new();
        if (!PhotonNetwork.OfflineMode)
        {
            _player.Weapon.GetPhotonView.RPC(stringBus.PlayerHideAim, RpcTarget.All, _player.PhotonView.ViewID);
        }

        Weapon weapon = _player.Weapon.GetCurrentWeapon;
        if (weapon != null)
        {
            if (weapon.Owner == _player.PhotonView.ViewID)
            {
                _trajectoryRenderer.ResetTrajectory();

                if (PhotonNetwork.OfflineMode)
                {
                    weapon.Shoot(InputVector, _player.transform.position, _player.transform.rotation);
                }
                else
                {
                    weapon.PhotonViewObject.RPC(stringBus.Shoot, RpcTarget.All, InputVector, _player.transform.position, _player.transform.rotation);
                }
            }
            else
            {
                _player.Weapon.DeleteWeapon(false);
            }
        }
    }

    protected override void OnPlayerMouseDowm()
    {
        if (!_player.gameObject.activeSelf) return;

        Weapon weapon = _player.Weapon.GetCurrentWeapon;
        if (weapon != null && weapon is IExplodable)
        {
            if (_updateTrajectory != null) StopCoroutine(_updateTrajectory);
            _updateTrajectory = StartCoroutine(UpdateTrajectory());
        }

        _player.Move.IsTakesAim = true;

        if (!PhotonNetwork.OfflineMode)
        {
            StringBus stringBus = new();
            _player.Weapon.GetPhotonView.RPC(stringBus.PlayerTakeAim, RpcTarget.Others);
        }
        else
        {
            if(_tutorialHand != null && _tutorialHand.activeSelf) _tutorialHand.SetActive(false);
        }
    }
}
