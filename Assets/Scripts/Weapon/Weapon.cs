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


    protected string Label;
    
    protected float Force;
    protected int Strength;

    protected float LifetimeSeconds;
    protected float Radius;

    protected LayerMask LayerEnemy;
    protected LayerMask LayerCells;

    public PlayerWeapon Player { get; protected set; }
    public int CurrentRoundLayerWhereImStay = -1;
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
        PlayerInfo player = other.GetComponent<PlayerInfo>();
        if (PhotonNetwork.IsMasterClient)
        {
            if (player && player.Weapon.GetCurrentWeapon is Punch && Owner == -1)
            {
                if(SpawnAnimation != null && !SpawnAnimation.IsComplete()) SpawnAnimation.Complete();
                if (player.Weapon.GetCurrentWeapon is Punch && player.Move.GetMoveActive)
                {
                    StringBus stringBus = new();

                    if (PhotonNetwork.OfflineMode) player.Weapon.GiveWeapon(PhotonViewObject.ViewID);
                    else player.PhotonView.RPC(stringBus.GiveWeapon, RpcTarget.All, PhotonViewObject.ViewID);
                    
                    if(player.PhotonView.IsMine) AudioSource.PlayClipAtPoint(AudioFX[(int)AudioType.Pickup], transform.position);
                }
            }
        }
    }
    #endregion

    private IEnumerator LifeTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        OnLifeTimeEnded();
    }

    protected virtual void OnLifeTimeEnded() { }

    [PunRPC]
    public virtual void Shoot(Vector2 target, Vector3 currentPos, Quaternion currentRotate)
    {
        UpdatePositionForWeapon(currentPos, currentRotate);

        if (LifetimeSeconds != -1) StartCoroutine(LifeTimer(LifetimeSeconds));
        Player.EnableAttackAnimation(WeaponType);
    }

    protected void Throw(Vector2 target)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        transform.parent = null;
        rigidbody.useGravity = true;
        rigidbody.mass = 2;
        rigidbody.drag = 1;
        rigidbody.isKinematic = false;
        rigidbody.AddForce(new Vector3(target.x, 0.5f, target.y) * 1000);
    }

    public int[] GetPlayersInRadius(Vector3 position, float radius)
    {
        int collidersCount = Physics.OverlapSphereNonAlloc(position, radius, _charactersClosed, LayerEnemy);
        int[] playersPhotonViewID = new int[collidersCount];

        for(int i = 0; i < collidersCount; i++)
        {
            PhotonView view = _charactersClosed[i].GetComponent<PhotonView>();
            if(view != null) playersPhotonViewID[i] = view.ViewID;
        }

        return playersPhotonViewID;
    }

    public virtual void Init(PlayerWeapon player)
    {
        Player = player;
        LayerEnemy = LayerMask.GetMask("Player");
        LayerCells = LayerMask.GetMask("Honeycomb");
        SetOwnerLocal(Player.GetPhotonView.ViewID);
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
        CurrentRoundLayerWhereImStay = -1;
        CurrentHoneycombWhereImStay = null;
        SetOwnerLocal(-1);
    }

    private void UpdatePositionForWeapon(Vector3 currentPos, Quaternion currentRotate)
    {
        Player.transform.position = currentPos;
        Player.transform.rotation = currentRotate;
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