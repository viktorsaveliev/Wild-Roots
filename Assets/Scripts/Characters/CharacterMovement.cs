using Photon.Pun;
using System.Collections;
using UnityEngine;

public class CharacterMovement : MonoBehaviour, IMoveable
{
    [SerializeField] private GameObject _rootsTroop;

    private const float _speed = 2f;
    private const float _rotateSpeed = 1f;

    private Rigidbody _rigidbody;
    private Animator _animator;
    private PhotonView _photonView;

    private bool _isMoveActive;
    private bool _isRootsActive;
    private float _moveInactiveTime;

    private Coroutine _freezeTimer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    public void Move(Vector3 direction)
    {
        Vector3 offset = _speed * Time.deltaTime * direction;
        _rigidbody.MovePosition(_rigidbody.position + offset);

        //_rigidbody.velocity = _speed * Time.deltaTime * direction;

        StringBus stringBus = new();
        _animator.SetFloat(stringBus.AnimationSpeed, _speed * Time.deltaTime);
    }

    public void Rotate(Vector3 direction)
    {
        if (Vector3.Angle(transform.forward, direction) > 0)
        {
            Vector3 newDiraction = Vector3.RotateTowards(transform.forward, direction, _rotateSpeed, 0);
            transform.rotation = Quaternion.LookRotation(newDiraction);
        }

        Quaternion rotation = transform.rotation;
        if (rotation.x != 0 || rotation.z != 0)
        {
            transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            _rigidbody.freezeRotation = true;
        }
    }

    public void SetMoveActive(bool active, float time = 2, bool activateRoots = false)
    {
        StringBus stringBus = new();
        if (active)
        {
            _isMoveActive = true;
            _moveInactiveTime = 0;
            if (!_photonView.IsMine) _rigidbody.freezeRotation = true;

            _animator.SetBool(stringBus.AnimationFall, false);

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
            _rigidbody.freezeRotation = false;

            if (_freezeTimer != null) StopCoroutine(_freezeTimer);
            _freezeTimer = StartCoroutine(FreezeTimer());

            if (activateRoots)
            {
                _rootsTroop.SetActive(true);
                _isRootsActive = true;
            }

            _animator.SetBool(stringBus.AnimationFall, true);
        }
    }

    private IEnumerator FreezeTimer()
    {
        yield return new WaitForSeconds(_moveInactiveTime);
        SetMoveActive(true);
    }

    public bool GetMoveActive() => _isMoveActive;
}
