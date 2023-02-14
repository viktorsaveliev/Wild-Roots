using UnityEngine;
using Photon.Pun;
using DG.Tweening;

[RequireComponent(typeof(PhotonView))]
public class PlayerWeapon : MonoBehaviour, IWeaponable
{
    [SerializeField] private Punch _punch;
    public Transform WeaponPosition;
    //public float CurrentCooldown = 0;

    private Weapon _currentWeapon;
    public Weapon GetCurrentWeapon => _currentWeapon;

    private PhotonView _photonView;
    public PhotonView GetPhotonView => _photonView;
    public Animator Animator { get; private set; }

    private void Start()
    {
        _photonView = PhotonView.Get(this);
        Animator = GetComponent<Animator>();
        EquipPunches();
    }

    /*private void Update()
    {
        if (!_photonView.IsMine) return;
        if (CurrentCooldown > 0f) CurrentCooldown -= Time.deltaTime;
    }*/

    [PunRPC]
    public void GiveWeapon(int id)
    {
        if (_currentWeapon != null && _currentWeapon != _punch) DeleteWeapon(true);

        PhotonView photonView = PhotonView.Find(id);
        _currentWeapon = photonView.GetComponent<Weapon>();
        _currentWeapon.transform.SetParent(WeaponPosition);
        _currentWeapon.Init(this);

        StringBus stringBus = new();
        Animator.SetBool(stringBus.AnimationWithWeapon, true);
        if (PhotonNetwork.IsMasterClient) ResetWeaponHoneycombData(_currentWeapon);

        EventBus.OnPlayerTakeWeapon?.Invoke();
    }

    [PunRPC]
    public void PlayerTakeAim()
    {
        EventBus.OnPlayerTakeAim?.Invoke(GetComponent<PlayerInfo>());
    }

    [PunRPC]
    public void PlayerHideAim(int id)
    {
        if (!PhotonView.Find(id).TryGetComponent<PlayerInfo>(out var player)) return;
        player.HUD.HideAimIndicator();
    }

    private void ResetWeaponHoneycombData(Weapon weapon)
    {
        if (weapon.CurrentHoneycombWhereImStay != null)
        {
            weapon.CurrentHoneycombWhereImStay.IsTiedWeapon = false;
            weapon.CurrentHoneycombWhereImStay = null;
        }
        weapon.CurrentRoundLayerWhereImStay = -1;
    }

    public void DeleteWeapon(bool destroyObject)
    {
        if(_currentWeapon != null && _currentWeapon != _punch)
        {
            if (destroyObject)
            {
                DestroyWeaponObject(_currentWeapon);
            }
            _currentWeapon.SetOwnerLocal(-1);
            EquipPunches();
        }
        StringBus stringBus = new();
        Animator.SetBool(stringBus.AnimationWithWeapon, false);
    }

    private void DestroyWeaponObject(Weapon weapon)
    {
        weapon.transform.DOScale(0.1f, 0.5f).OnComplete(() =>
        {
            weapon.transform.parent = null;
            weapon.gameObject.SetActive(false);
        });
    }

    private void EquipPunches()
    {
        _currentWeapon = _punch;
        _currentWeapon.Init(this);
    }

    public void EnableAttackAnimation(WeaponsHandler.WeaponType weapon)
    {
        StringBus stringBus = new();
        switch (weapon)
        {
            case WeaponsHandler.WeaponType.Grenade:
            case WeaponsHandler.WeaponType.RootsMine:

                Animator.SetTrigger(stringBus.AnimationAttackGrenade);
                break;

            case WeaponsHandler.WeaponType.Punch:
                Animator.SetTrigger(stringBus.AnimationPunch);
                break;
        }
    }
}