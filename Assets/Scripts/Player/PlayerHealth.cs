using Photon.Pun;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInfo))]
public class PlayerHealth : MonoBehaviour
{
    public int Health { get; private set; }
    public float DamageStrength { get; private set; }
    public float DamageMultiplier { get; private set; }
    public Weapon FromWhomDamage;
    private PlayerInfo _playerInfo;

    private void OnEnable()
    {
        EventBus.OnPlayerTakeDamage += StartTimerForResetDamageInfo;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerTakeDamage -= StartTimerForResetDamageInfo;
    }

    private void Start()
    {
        Health = 3;
        DamageStrength = 0;
        DamageMultiplier = 1000f;
        _playerInfo = GetComponent<PlayerInfo>();
    }

    public void PlayerFell()
    {
        if(GameSettings.GameMode == SelectGameMode.GameMode.PvP)
        {
            if (--Health <= 0)
            {
                EventBus.OnPlayerLose?.Invoke();
            }
        }
        
        if(_playerInfo.Weapon) _playerInfo.Weapon.DeleteWeapon(true);
        gameObject.SetActive(false);
        EventBus.OnPlayerFall?.Invoke(_playerInfo, Health);
    }

    public void SetDamageStrength(float plus)
    {
        if (GameSettings.GameMode != SelectGameMode.GameMode.PvP) return;
        DamageStrength += plus;
        if (DamageStrength > 1f) DamageStrength = 1f;
    }

    public void StartTimerForResetDamageInfo(PlayerInfo player)
    {
        StartCoroutine(TimerForResetDamageInfo(player));
    }

    public IEnumerator TimerForResetDamageInfo(PlayerInfo player)
    {
        if (player == _playerInfo)
        {
            yield return new WaitForSeconds(5f);
            FromWhomDamage = null;
        }
    }

    public float GetDamageMultiplie() => (DamageStrength * DamageMultiplier);

    /*[PunRPC]
    public void SetImpulseFromBullet(float force, Vector3 impulsePosition, int fromWhom, int viewID)
    {
        PlayerInfo playerControl = PhotonView.Find(viewID).GetComponent<PlayerInfo>();
        Weapon weapon = PhotonView.Find(fromWhom).GetComponent<Weapon>();

        playerControl.Health.SetDamageStrength(0.05f);
        playerControl.Health.FromWhomDamage = weapon;

        playerControl.Rigidbody.AddForce(impulsePosition * (force + playerControl.Health.GetDamageDamageMultiplie()));
        //playerControl.PlayerMove.SetMoveActive(false);

        EventBus.OnPlayerTakeDamage?.Invoke(playerControl);
    }*/
}
