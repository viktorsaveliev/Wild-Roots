using UnityEngine;
using Photon.Pun;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
public class PlayerMove : MonoBehaviour, IPunObservable, IMoveable
{
    [SerializeField] private GameObject _rootsTroop;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotateSpeed;

    public bool IsTakesAim;
    private bool _isMoveActive;
    private bool _isRootsActive;
    private float _moveInactiveTime;

    public bool IsCanRotate { get; private set; }

    public bool IsGrounded;

    private Coroutine _freezeTimer;
    private Character _character;

    private Vector3 correctPlayerPosition;
    private Quaternion correctPlayerRotation;

    private Coroutine _rotateDelayCoroutine;
    private int _playerDevice;

    private void OnEnable()
    {
        EventBus.OnPlayerDisconnected += DisableMovement;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerDisconnected -= DisableMovement;
    }

    private void Start()
    {
        _character = GetComponent<Character>();
        IsCanRotate = true;

        StringBus stringBus = new();
        _playerDevice = PlayerPrefs.GetInt(stringBus.PlayerDevice);

        Invoke(nameof(SetCanMovePlayer), 1);
    }

    private void Update()
    {
        if(!_character.PhotonView.IsMine)
        {
            //transform.DOMove(this.correctPlayerPosition, 0.4f);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, this.correctPlayerPosition, Time.deltaTime * 10), 
                Quaternion.Lerp(transform.rotation, this.correctPlayerRotation, Time.deltaTime * 10));
        }
        else
        {
            if(_playerDevice == 1 && _isMoveActive)
            {
                float moveHorizontal = Input.GetAxis("Horizontal");
                float moveVertical = Input.GetAxis("Vertical");

                if (moveHorizontal == 0 && moveVertical == 0)
                {
                    StringBus stringBus = new();
                    _character.Animator.SetFloat(stringBus.AnimationSpeed, 0);
                }
                else
                {
                    Move(new Vector3(moveHorizontal, 0, moveVertical));
                    if (!IsTakesAim && IsCanRotate) Rotate(new Vector3(moveHorizontal, 0, moveVertical));
                }
            }
        }
    }

    private void SetCanMovePlayer() => SetMoveActive(true);
    public void Move(Vector3 direction)
    {
        Vector3 offset = _moveSpeed * Time.deltaTime * direction;
        _character.Rigidbody.MovePosition(_character.Rigidbody.position + offset);

        StringBus stringBus = new();
        _character.Animator.SetFloat(stringBus.AnimationSpeed, _moveSpeed * Time.deltaTime);
    }

    public void Stop()
    {
        StringBus stringBus = new();
        _character.Animator.SetFloat(stringBus.AnimationSpeed, 0);
    }

    public void Rotate(Vector3 direction)
    {
        if(Vector3.Angle(transform.forward, direction) > 0)
        {
            Vector3 newDiraction = Vector3.RotateTowards(transform.forward, direction, _rotateSpeed, 0);
            transform.rotation = Quaternion.LookRotation(newDiraction);
        }

        Quaternion rotation = transform.rotation;
        if (rotation.x != 0 || rotation.z != 0)
        {
            transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            _character.Rigidbody.freezeRotation = true;
        }
    }

    public void SetDelayToRotate()
    {
        if (!IsCanRotate) StopCoroutine(_rotateDelayCoroutine);

        IsCanRotate = false;
        float delatInSeconds = 0.5f;
        _rotateDelayCoroutine = StartCoroutine(RotateDelay(delatInSeconds));
    }

    private IEnumerator RotateDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        IsCanRotate = true;
    }

    public void SetMoveActive(bool active, float time = 2, bool activateRoots = false)
    {
        StringBus stringBus = new();
        if(active)
        {
            _isMoveActive = true;
            _moveInactiveTime = 0;
            if(!_character.PhotonView.IsMine) _character.Rigidbody.freezeRotation = true;

            _character.Animator.SetBool(stringBus.AnimationFall, false);

            if (_isRootsActive)
            {
                _rootsTroop.SetActive(false);
                _isRootsActive = false;
            }
        }
        else
        {
            _isMoveActive = false;
            _moveInactiveTime = time;
            _character.Rigidbody.freezeRotation = false;

            if (_freezeTimer != null) StopCoroutine(_freezeTimer);
            _freezeTimer = StartCoroutine(FreezeTimer());

            if (activateRoots)
            {
                _rootsTroop.SetActive(true);
                _isRootsActive = true;
            }
           
            _character.Animator.SetBool(stringBus.AnimationFall, true);
        }
        EventBus.OnPlayerChangedMoveState?.Invoke(_character, active);
    }

    public bool GetMoveActive() => _isMoveActive;

    private IEnumerator FreezeTimer()
    {
        yield return new WaitForSeconds(_moveInactiveTime);
        SetMoveActive(true);
    }

    private void DisableMovement()
    {
        if (!_character.PhotonView.IsMine) return;
        SetMoveActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            this.correctPlayerPosition = (Vector3)stream.ReceiveNext();
            this.correctPlayerRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
