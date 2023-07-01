using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerHealth : MonoBehaviour
{
    public int Value { get; private set; }
    public float DamageStrength { get; private set; }
    public float DamageMultiplier { get; private set; }
    [HideInInspector] public int FromWhomDamage;
    private Character _character;
    private Coroutine _resetInfoTimer;

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
        FromWhomDamage = -1;
        Value = 3;
        DamageStrength = 0;
        DamageMultiplier = 1000f;
        _character = GetComponent<Character>();
    }

    public void PlayerFell()
    {
        if (--Value <= 0)
        {
            EventBus.OnCharacterLose?.Invoke(_character.PhotonView.ViewID);
        }

        if(_character.Weapon) _character.Weapon.DeleteWeapon(true);
        gameObject.SetActive(false);
        EventBus.OnCharacterFall?.Invoke(_character, Value);
    }
    
    public void SetDamageStrength(float plus)
    {
        DamageStrength += plus;
        if (DamageStrength > 1f) DamageStrength = 1f;
    }

    private void StartTimerForResetDamageInfo(Character character)
    {
        if (character != _character) return;

        if (_resetInfoTimer != null)
        {
            StopCoroutine(_resetInfoTimer);
        }
        _resetInfoTimer = StartCoroutine(TimerForResetDamageInfo());
    }

    private IEnumerator TimerForResetDamageInfo()
    {
        yield return new WaitForSeconds(6f);
        FromWhomDamage = -1;
    }

    public float GetDamageMultiplie() => DamageStrength * DamageMultiplier;
}
