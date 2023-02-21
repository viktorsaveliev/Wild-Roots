using Photon.Pun;
using UnityEngine;
using System.Collections;

public class JoystickAttack : JoystickHandler
{
    [SerializeField] private GameObject _tutorialHand;

    private TrajectoryRenderer _trajectoryRenderer;
    private Coroutine _updateTrajectory;

    private Character _character;
    private bool _isInit = false;

    protected override void OnPlayerMouseDrag()
    {
        if (!_character.gameObject.activeSelf || !_isInit || !_character.PhotonView.IsMine || !_character.Move.GetMoveActive()) return;
        if (InputVector.x != 0 || InputVector.y != 0)
        {
            _character.Move.Rotate(new Vector3(InputVector.x, 0, InputVector.y));
        }
    }

    private IEnumerator UpdateTrajectory()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Vector3 playerPos = new(_character.transform.position.x, _character.transform.position.y + 0.5f, _character.transform.position.z);
            _trajectoryRenderer.ShowTrajectory(playerPos, new Vector3(InputVector.x, 0.5f, InputVector.y) * 8);
        }
    }

    public void Init(Character character)
    {
        _character = character;
        _trajectoryRenderer = GetComponent<TrajectoryRenderer>();
        _isInit = true;
    }

    protected override void OnPlayerMouseUp()
    {
        if (!_character.gameObject.activeSelf) return;

        if (_updateTrajectory != null)
        {
            StopCoroutine(_updateTrajectory);
            _updateTrajectory = null;
        }

        _character.Move.IsTakesAim = false;
        _character.Move.SetDelayToRotate();

        StringBus stringBus = new();
        if (!PhotonNetwork.OfflineMode)
        {
            _character.Weapon.GetPhotonView().RPC(stringBus.PlayerHideAim, RpcTarget.All, _character.PhotonView.ViewID);
        }

        Weapon weapon = _character.Weapon.GetCurrentWeapon();
        if (weapon != null)
        {
            if (weapon.Owner == _character.PhotonView.ViewID)
            {
                _trajectoryRenderer.ResetTrajectory();

                Vector3 inputVector = new(InputVector.x, InputVector.y, 0);
                if (PhotonNetwork.OfflineMode)
                {
                    weapon.Shoot(inputVector, _character.transform.position, _character.transform.rotation);
                }
                else
                {
                    weapon.PhotonViewObject.RPC(stringBus.Shoot, RpcTarget.All, inputVector, _character.transform.position, _character.transform.rotation, false);
                }
            }
            else
            {
                _character.Weapon.DeleteWeapon(false);
            }
        }
    }

    protected override void OnPlayerMouseDowm()
    {
        if (!_character.gameObject.activeSelf) return;

        Weapon weapon = _character.Weapon.GetCurrentWeapon();
        if (weapon != null && weapon is IExplodable)
        {
            if (_updateTrajectory != null) StopCoroutine(_updateTrajectory);
            _updateTrajectory = StartCoroutine(UpdateTrajectory());
        }

        _character.Move.IsTakesAim = true;

        if (!PhotonNetwork.OfflineMode)
        {
            StringBus stringBus = new();
            _character.Weapon.GetPhotonView().RPC(stringBus.PlayerTakeAim, RpcTarget.Others);
        }
        else
        {
            if(_tutorialHand != null && _tutorialHand.activeSelf) _tutorialHand.SetActive(false);
        }
    }
}
