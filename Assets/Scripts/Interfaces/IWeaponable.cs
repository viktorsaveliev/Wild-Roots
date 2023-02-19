using Photon.Pun;

public interface IWeaponable
{
    public void GiveWeapon(int id);
    public void DeleteWeapon(bool destroyObject);
    public Weapon GetCurrentWeapon();
    public void TakeAim();
    public void HideAim(int id);
    public void EquipPunches();
    public PhotonView GetPhotonView();

}
