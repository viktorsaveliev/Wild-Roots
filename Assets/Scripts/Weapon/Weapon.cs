using UnityEngine;
using Photon.Pun;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected ParticleSystem PrefabAttackFX;
    [SerializeField] protected AudioClip[] AudioFX;

    protected enum AudioType
    {
        Pickup,
        Explode
    }

    public bool IsActive { get; protected set; }
    protected string Label;
    
    protected float Force;
    protected int Strength;

    protected float LifetimeSeconds;
    protected float Radius;

    //protected LayerMask CharacterLayer;

    public CharacterWeapon CharacterOwner { get; protected set; }
    public SpawnPoint CurrentSpawnPoint;
    public WeaponSpawner.WeaponType WeaponType;

    public PhotonView PhotonView { get; private set; }
    public int Owner { get; private set; }

    private Collider[] _targetsClosed = new Collider[15];
    private AnimateObject _animation;
    private Rigidbody _rigidbody;

    #region MonoBehavior
    private void Awake()
    {
        _animation = GetComponent<AnimateObject>();
        _rigidbody = GetComponent<Rigidbody>();
        PhotonView = PhotonView.Get(this);

        SetOwnerLocal(-1);
    }

    private void OnTriggerEnter(Collider other)
    {
        IWeaponable character = other.GetComponent<IWeaponable>();
        if (character != null && character.GetCurrentWeapon() is Punch && Owner == -1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                AssignToPlayer(character);
            }
            else
            {
                if(character.GetPhotonView().IsMine && PhotonNetwork.OfflineMode == false)
                {
                    StringBus stringBus = new();
                    character.GetPhotonView().RPC(stringBus.AskForAWeapon, RpcTarget.MasterClient, character.GetPhotonView().ViewID, PhotonView.ViewID);
                }
            }
        }
    }
    #endregion

    public void AssignToPlayer(IWeaponable character)
    {
        IMoveable characterMove = character.GetPhotonView().gameObject.GetComponent<IMoveable>();
        if (character.GetCurrentWeapon() is Punch && characterMove.IsCanMove())
        {
            if (PhotonNetwork.OfflineMode) character.GiveWeapon(this);
            else
            {
                StringBus stringBus = new();
                character.GetPhotonView().RPC(stringBus.GiveWeapon, RpcTarget.All, PhotonView.ViewID);
            }
            AudioSource.PlayClipAtPoint(AudioFX[(int)AudioType.Pickup], transform.position);
        }
    }

    protected virtual IEnumerator LifeTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        OnLifeTimeEnded();
    }

    protected virtual void OnLifeTimeEnded() { }

    [PunRPC]
    public virtual void Shoot(Vector3 target, Vector3 currentPos, Quaternion currentRotate, bool isABot = false)
    {
        UpdatePositionForWeapon(currentPos, currentRotate);

        if (LifetimeSeconds != -1 && gameObject.activeSelf)
        {
            StartCoroutine(LifeTimer(LifetimeSeconds));
        }
        IsActive = true;
        CharacterOwner.EnableAttackAnimation(WeaponType);
    }

    protected void Throw(Vector3 target, bool isABot)
    {
        transform.parent = null;
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(new Vector3(target.x, 0.5f, isABot ? target.z : target.y) * 2000);
    }

    public int[] GetTargetsInRadius(Vector3 position, float radius)
    {
        int collidersCount = Physics.OverlapSphereNonAlloc(position, radius, _targetsClosed); // CharacterLayer
        int[] targetsPhotonViewID = new int[collidersCount];

        for(int i = 0; i < collidersCount; i++)
        {
            if (_targetsClosed[i].TryGetComponent<PhotonView>(out var view))
            {
                targetsPhotonViewID[i] = view.ViewID;
            }
            else
            {
                targetsPhotonViewID[i] = -1;
            }
        }

        return targetsPhotonViewID;
    }

    public virtual void Init(CharacterWeapon character)
    {
        CharacterOwner = character;
        //CharacterLayer = LayerMask.GetMask("Player");
        SetOwnerLocal(CharacterOwner.GetPhotonView().ViewID);

        if(_animation != null) _animation.StopTweenAnimate();
    }

    protected void SetLocalPosAndRotate(Vector3 pos, Quaternion rotate)
    {
        transform.localPosition = pos;
        transform.localRotation = rotate;
    }

    protected void PlayAttackFX()
    {
        if(PrefabAttackFX != null) PrefabAttackFX.Play();
    }

    public void SetOwnerLocal(int id)
    {
        Owner = id;
    }

    public void DisableWeapon()
    {
        for(int i = 0; i < _targetsClosed.Length; i++)
        {
            _targetsClosed[i] = null;
        }

        SetOwnerLocal(-1);
        CurrentSpawnPoint = null;
        IsActive = false;
        gameObject.SetActive(false);
    }

    private void UpdatePositionForWeapon(Vector3 currentPos, Quaternion currentRotate)
    {
        CharacterOwner.transform.SetPositionAndRotation(currentPos, currentRotate);
    }
}