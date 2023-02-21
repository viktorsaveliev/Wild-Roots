using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class CharacterWeapon : MonoBehaviour, IWeaponable
{

    [SerializeField] private Punch _punch;
    public Transform WeaponPosition;

    private Weapon _currentWeapon;

    private PhotonView _photonView;
    public Animator Animator { get; private set; }

    private void Start()
    {
        _photonView = PhotonView.Get(this);
        Animator = GetComponent<Animator>();
        EquipPunches();
    }

    public Weapon GetCurrentWeapon() => _currentWeapon;

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

    public void EquipPunches()
    {
        _currentWeapon = _punch;
        _currentWeapon.Init(this);
    }

    [PunRPC]
    public void DeleteWeapon(bool destroyObject)
    {
        if (_currentWeapon != null && _currentWeapon != _punch)
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

    [PunRPC]
    public void HideAim(int id)
    {
        if (!PhotonView.Find(id).TryGetComponent<Character>(out var character)) return;
        character.HUD.HideAimIndicator();
    }

    [PunRPC]
    public void TakeAim()
    {
        EventBus.OnCharacterTakeAim?.Invoke(GetComponent<Character>());
    }

    public PhotonView GetPhotonView() => _photonView;

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

    private void ResetWeaponHoneycombData(Weapon weapon)
    {
        if (weapon.CurrentHoneycombWhereImStay != null)
        {
            weapon.CurrentHoneycombWhereImStay.IsTiedWeapon = false;
            weapon.CurrentHoneycombWhereImStay = null;
        }
        weapon.CurrentLayerWhereImStay = -1;
    }
}
