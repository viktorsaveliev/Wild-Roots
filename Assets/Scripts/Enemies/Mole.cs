/*using Photon.Pun;
using UnityEngine;

public class Mole : Enemy, IWeaponable
{
    private Weapon _currentWeapon;

    [PunRPC]
    public void GiveWeapon(int id)
    {
        if (_currentWeapon != null) DeleteWeapon(true);

        PhotonView photonView = PhotonView.Find(id);
        _currentWeapon = photonView.GetComponent<Weapon>();
        _currentWeapon.transform.SetParent(WeaponPosition);
        _currentWeapon.Init(this);

        StringBus stringBus = new();
        Animator.SetBool(stringBus.AnimationWithWeapon, true);
        if (PhotonNetwork.IsMasterClient) ResetWeaponHoneycombData(_currentWeapon);
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
        if (_currentWeapon != null)
        {
            if (destroyObject)
            {
                DestroyWeaponObject(_currentWeapon);
            }
            _currentWeapon.SetOwnerLocal(-1);
            _currentWeapon = null;
        }
        StringBus stringBus = new();
        Animator.SetBool(stringBus.AnimationWithWeapon, false);
    }
}*/
