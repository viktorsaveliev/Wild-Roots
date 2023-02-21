using Photon.Pun;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerHealth : MonoBehaviour
{
    public int Health { get; private set; }
    public float DamageStrength { get; private set; }
    public float DamageMultiplier { get; private set; }
    public Weapon FromWhomDamage;
    private Character _character;

    private void OnEnable()
    {
        EventBus.OnCharacterTakeDamage += StartTimerForResetDamageInfo;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterTakeDamage -= StartTimerForResetDamageInfo;
    }

    private void Start()
    {
        Health = 3;
        DamageStrength = 0;
        DamageMultiplier = 1000f;
        _character = GetComponent<Character>();
    }

    public void PlayerFell()
    {
        if(GameSettings.GameMode == GameModeSelector.GameMode.PvP)
        {
            if (--Health <= 0)
            {
                EventBus.OnCharacterLose?.Invoke();
            }
            print(Health);
        }
        
        if(_character.Weapon) _character.Weapon.DeleteWeapon(true);
        gameObject.SetActive(false);
        EventBus.OnCharacterFall?.Invoke(_character, Health);
    }

    public void SetDamageStrength(float plus)
    {
        if (GameSettings.GameMode != GameModeSelector.GameMode.PvP) return;
        DamageStrength += plus;
        if (DamageStrength > 1f) DamageStrength = 1f;
    }

    public void StartTimerForResetDamageInfo(Character character)
    {
        StartCoroutine(TimerForResetDamageInfo(character));
    }

    public IEnumerator TimerForResetDamageInfo(Character character)
    {
        if (character == _character)
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
