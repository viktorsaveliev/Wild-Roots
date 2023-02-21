using Photon.Pun;

public interface IWeaponable
{
    [PunRPC]
    public void GiveWeapon(int id);

    [PunRPC]
    public void DeleteWeapon(bool destroyObject);

    public Weapon GetCurrentWeapon();

    [PunRPC]
    public void TakeAim();

    [PunRPC]
    public void HideAim(int id);

    public void EquipPunches();

    public PhotonView GetPhotonView();
}
