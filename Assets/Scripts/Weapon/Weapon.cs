using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(AnimateObject))]
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected ParticleSystem PrefabAttackFX;
    [SerializeField] protected AudioClip[] AudioFX;

    protected enum AudioType
    {
        Pickup,
        Explode
    }

    [SerializeField] private Sprite SpriteIcon;
    public Sprite GetSpriteIcon => SpriteIcon;

    public bool IsActive { get; protected set; }
    protected string Label;
    
    protected float Force;
    protected int Strength;

    protected float LifetimeSeconds;
    protected float Radius;

    protected LayerMask LayerEnemy;
    protected LayerMask LayerCells;

    public CharacterWeapon CharacterOwner { get; protected set; }
    public int CurrentLayerWhereImStay = -1;
    public Honeycomb CurrentHoneycombWhereImStay;
    public WeaponsHandler.WeaponType WeaponType;

    public Tween SpawnAnimation;

    public PhotonView PhotonViewObject { get; private set; }
    public int Owner { get; private set; }

    private Collider[] _charactersClosed = new Collider[5];

    #region MonoBehavior
    private void Awake()
    {
        PhotonViewObject = PhotonView.Get(this);
        SetOwnerLocal(-1);
    }

    private void OnTriggerEnter(Collider other)
    {
        IWeaponable character = other.GetComponent<IWeaponable>();
        if (character != null && character.GetCurrentWeapon() is Punch && Owner == -1)
        {
            StringBus stringBus = new();
            if (PhotonNetwork.IsMasterClient)
            {
                AssignToPlayer(character);
            }
            else
            {
                if(character.GetPhotonView().IsMine)
                {
                    character.GetPhotonView().RPC(stringBus.AskForAWeapon, RpcTarget.MasterClient, character.GetPhotonView().ViewID, PhotonViewObject.ViewID);
                }
            }
        }
    }
    #endregion

    public void AssignToPlayer(IWeaponable character)
    {
        IMoveable characterMove = character.GetPhotonView().gameObject.GetComponent<IMoveable>();
        if (SpawnAnimation != null && !SpawnAnimation.IsComplete()) SpawnAnimation.Complete();
        if (character.GetCurrentWeapon() is Punch && characterMove.IsCanMove())
        {
            if (PhotonNetwork.OfflineMode) character.GiveWeapon(PhotonViewObject.ViewID);
            else
            {
                StringBus stringBus = new();
                character.GetPhotonView().RPC(stringBus.GiveWeapon, RpcTarget.All, PhotonViewObject.ViewID);
            }
        }
        AudioSource.PlayClipAtPoint(AudioFX[(int)AudioType.Pickup], transform.position);
    }

    private IEnumerator LifeTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        OnLifeTimeEnded();
    }

    protected virtual void OnLifeTimeEnded() { }

    [PunRPC]
    public virtual void Shoot(Vector3 target, Vector3 currentPos, Quaternion currentRotate, bool isABot = false)
    {
        UpdatePositionForWeapon(currentPos, currentRotate);

        if (LifetimeSeconds != -1) StartCoroutine(LifeTimer(LifetimeSeconds));
        IsActive = true;
        CharacterOwner.EnableAttackAnimation(WeaponType);
    }

    protected void Throw(Vector3 target, bool isABot)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        transform.parent = null;
        rigidbody.useGravity = true;
        rigidbody.mass = 2;
        rigidbody.drag = 1;
        rigidbody.isKinematic = false;

        rigidbody.AddForce(new Vector3(target.x, 0.5f, isABot ? target.z : target.y) * 1000);
    }

    public int[] GetPlayersInRadius(Vector3 position, float radius)
    {
        int collidersCount = Physics.OverlapSphereNonAlloc(position, radius, _charactersClosed, LayerEnemy);
        int[] playersPhotonViewID = new int[collidersCount];

        for(int i = 0; i < collidersCount; i++)
        {
            if(_charactersClosed[i].TryGetComponent<PhotonView>(out var view)) playersPhotonViewID[i] = view.ViewID;
        }

        return playersPhotonViewID;
    }

    public virtual void Init(CharacterWeapon character)
    {
        CharacterOwner = character;
        LayerEnemy = LayerMask.GetMask("Player");
        LayerCells = LayerMask.GetMask("Honeycomb");
        SetOwnerLocal(CharacterOwner.GetPhotonView().ViewID);
        GetComponent<AnimateObject>().StopTweenAnimate();
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
        gameObject.SetActive(false);
        CurrentLayerWhereImStay = -1;
        CurrentHoneycombWhereImStay = null;
        IsActive = false;
        SetOwnerLocal(-1);
    }

    private void UpdatePositionForWeapon(Vector3 currentPos, Quaternion currentRotate)
    {
        CharacterOwner.transform.SetPositionAndRotation(currentPos, currentRotate);
    }

    /*public Honeycomb[] GetCellsInRadius(Vector3 position, float radius)
    {
        Collider[] cellClosed = Physics.OverlapSphere(position, radius, LayerCells);
        
        Honeycomb[] cells = new Honeycomb[cellClosed.Length];
        for(int i = 0; i < cellClosed.Length; i++)
        {
            cells[i] = cellClosed[i].GetComponent<Honeycomb>();
        }

        return cells;
    }*/
}