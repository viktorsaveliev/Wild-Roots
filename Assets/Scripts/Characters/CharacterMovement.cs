using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour, IPunObservable, IMoveable
{
    [SerializeField] private GameObject _rootsTroop;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotateSpeed;

    private NavMeshAgent _agent;

    public bool IsTakesAim;
    private bool _isMoveActive;
    private bool _isRootsActive;
    private float _freezeTime;
    public float GetFreezeTime => _freezeTime;

    public bool IsCanRotate { get; private set; }

    private Coroutine _freezeTimer;
    private Character _character;
    private CharacterAI _behaviour;

    private Vector3 correctPlayerPosition;
    private Quaternion correctPlayerRotation;

    private Coroutine _rotateDelayCoroutine;
    private int _playerDevice;
    private bool _isOnGround;
    private bool _needGetsOnFeet;

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

        if(_character.IsABot == false)
        {
            IsCanRotate = true;

            StringBus stringBus = new();
            _playerDevice = PlayerPrefs.GetInt(stringBus.PlayerDevice);
        }
        else
        {
            _agent = GetComponent<NavMeshAgent>();
            _behaviour = GetComponent<CharacterAI>();

            _agent.speed = _moveSpeed;
            //_isMoveActive = true;
        }
        SetMoveActive(false, 3, false, true);
    }

    private void FixedUpdate()
    {
        if (_character.PhotonView.IsMine == false && PhotonNetwork.OfflineMode == false)
        {
            if (PhotonNetwork.IsMasterClient && _character.IsABot) return;
            //transform.DOMove(this.correctPlayerPosition, 0.4f);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, this.correctPlayerPosition, Time.deltaTime * 10),
                Quaternion.Lerp(transform.rotation, this.correctPlayerRotation, Time.deltaTime * 10));
        }
        else
        {
            if (_playerDevice == 1 && _isMoveActive)
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
                    if (IsTakesAim == false && IsCanRotate && _needGetsOnFeet == false) Rotate(new Vector3(moveHorizontal, 0, moveVertical));
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7 && _freezeTime == -1)
        {
            if (_isMoveActive == false && _isOnGround == false)
            {
                _character.transform.DOPunchScale(new Vector3(0.2f, 0.2f), 0.5f).OnComplete(() => _character.transform.localScale = new Vector3(2, 2, 2));
                SetMoveActive(false, 1f);
                _isOnGround = true;
            }
        }
    }

    public void Move(Vector3 direction)
    {
        if (_needGetsOnFeet)
        {
            StandUpOnFeet();
        }
        else
        {
            if (_character.IsABot)
            {
                if (_agent.isOnNavMesh == false) return;
                _behaviour.IsHavePath = true;
                _agent.SetDestination(direction);
            }
            else
            {
                Vector3 offset = _moveSpeed * Time.deltaTime * direction;
                _character.Rigidbody.MovePosition(_character.Rigidbody.position + offset);
            }
            StringBus stringBus = new();
            _character.Animator.SetFloat(stringBus.AnimationSpeed, _moveSpeed * Time.deltaTime);
        }
    }

    private bool _getsOnFeet;
    private void StandUpOnFeet()
    {
        if (_needGetsOnFeet == false && _getsOnFeet) return;
        _getsOnFeet = true;

        transform.DORotate(new Vector3(transform.rotation.x, 0, transform.rotation.z), 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            _needGetsOnFeet = false;
            _getsOnFeet = false;

            if (_character.IsABot && transform.position.y > 0)
            {
                _character.Rigidbody.freezeRotation = true;
                _agent.enabled = true;
            }
        });
    }

    private void DisableMovement()
    {
        if (!_character.PhotonView.IsMine || _character.IsABot) return;
        SetMoveActive(false);
    }

    public void Stop()
    {
        if(_character.IsABot) _behaviour.IsHavePath = false;

        StringBus stringBus = new();
        _character.Animator.SetFloat(stringBus.AnimationSpeed, 0);
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

    public void SetMoveActive(bool active, float time = 2, bool activateRoots = false, bool freezeRotate = false)
    {
        StringBus stringBus = new();
        if (active)
        {
            if (transform.position.y < -0.5f) return;

            _isMoveActive = true;
            _freezeTime = 0;

            if (transform.rotation.y != 0)
            {
                _needGetsOnFeet = true;
                if (_character.IsABot) StandUpOnFeet();
            }
            else
            {
                if (_character.IsABot) _agent.enabled = true;
            }

            if (!_character.PhotonView.IsMine) _character.Rigidbody.freezeRotation = true;

            Stop();
            _character.Animator.SetBool(stringBus.AnimationFall, false);

            if (_isRootsActive)
            {
                _rootsTroop.SetActive(false);
                _isRootsActive = false;
            }
            _isOnGround = false;
        }
        else
        {
            if (_character.IsABot) _agent.enabled = false;

            _isMoveActive = false;
            _freezeTime = time;
            _character.Rigidbody.freezeRotation = freezeRotate;

            if(gameObject.activeSelf && _freezeTime > 0)
            {
                if (_freezeTimer != null) StopCoroutine(_freezeTimer);
                _freezeTimer = StartCoroutine(FreezeTimer());
            }
            
            if (activateRoots)
            {
                _rootsTroop.SetActive(true);
                _isRootsActive = true;
            }
            Stop();
            _character.Animator.SetBool(stringBus.AnimationFall, true);
        }
    }

    private IEnumerator FreezeTimer()
    {
        yield return new WaitForSeconds(_freezeTime);
        SetMoveActive(true);
    }

    public bool IsCanMove() => _isMoveActive;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
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
